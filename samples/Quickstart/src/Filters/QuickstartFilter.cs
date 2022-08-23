using Azure.Health.DataServices.Clients;
using Azure.Health.DataServices.Filters;
using Azure.Health.DataServices.Json;
using Azure.Health.DataServices.Json.Transforms;
using Azure.Health.DataServices.Pipelines;
using Azure.Health.DataServices.Protocol;
using Azure.Health.DataServices.Security;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QuickstartSample.CustomHeader;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Quickstart.Filters
{
    public class QuickstartFilter : IInputFilter
    {
        public QuickstartFilter(IOptions<QuickstartOptions> options, IAuthenticator authenticator, ICustomHeaderService customHeader, TelemetryClient telemetryClient = null, ILogger<QuickstartFilter> logger = null)
        {
            id = Guid.NewGuid().ToString();
            tokenBag = new();
            fhirServerUrl = options.Value.FhirServerUrl;
            pageSize = options.Value.PageSize;
            maxSize = options.Value.MaxSize;
            retryDelaySeconds = options.Value.RetryDelaySeconds;
            maxRetryAttempts = options.Value.MaxRetryAttempts;
            status = options.Value.ExecutionStatusType;
            this.authenticator = authenticator;
            this.telemetryClient = telemetryClient;
            this.logger = logger;
            this.customHeader = customHeader;
        }

        private readonly StatusType status;
        private readonly string fhirServerUrl;
        private readonly int pageSize;
        private readonly int maxSize;
        private readonly double retryDelaySeconds;
        private readonly int maxRetryAttempts;
        private readonly IAuthenticator authenticator;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger logger;
        private readonly List<JToken> tokenBag;
        private readonly string id;
        private readonly ICustomHeaderService customHeader;
        public string Id => id;

        public string Name => "PatientSampleFilter";

        public StatusType ExecutionStatusType => status;

        public event EventHandler<FilterErrorEventArgs> OnFilterError;

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            DateTime start = DateTime.Now;
            if (context == null || context.IsFatal)
            {
                return context;
            }

            string securityToken = await authenticator.AcquireTokenForClientAsync(fhirServerUrl);
            JToken pageToken = null;
            FhirUriPath fpath = new(context.Request.Method.ToString(), context.Request.RequestUri.ToString());
            var headers = customHeader.GetHeaders(context.Request);

            try
            {
                if (context.Request.Method == HttpMethod.Get)
                {
                    JToken resourceToken = await GetResourceAsync(fpath.Resource, $"_id={fpath.Id}", headers, securityToken);
                    logger?.LogDebug("{Name}-{Id} Resource token returned.", Name, Id);

                    AddTokensToBag(resourceToken.GetArray("$.entry", false));
                    logger?.LogDebug("{Name}-{Id} Entry tokens added to bag.", Name, Id);

                    JToken resourceEntryToken = await GetResourceAsync($"{fpath.Resource}/{fpath.Id}/*", $"_count={pageSize}", headers, securityToken, false);
                    logger?.LogDebug("{Name}-{Id} Resource token from id returned to confirm exists.", Name, Id);

                    if (!resourceEntryToken.IsNullOrEmpty() && !resourceEntryToken.IsNullOrEmpty("$.entry"))
                    {
                        logger?.LogDebug("{Name}-{Id} Resource entry token exists.", Name, Id);
                        AddTokensToBag(resourceEntryToken.GetArray("$.entry", true));
                        logger?.LogDebug("{Name}-{Id} Entry tokens added to bag.", Name, Id);

                        bool more = HasMore(resourceEntryToken);
                        logger?.LogDebug("{Name}-{Id} get more entries {more}.", Name, Id, more);

                        while (more && tokenBag.Count <= maxSize)
                        {
                            string pagePath = GetPagePath(resourceEntryToken);
                            logger?.LogDebug("{Name}-{Id} got page path {pagePath}.", Name, Id, pagePath);

                            pageToken = await GetResourceAsync(pagePath, null, headers, securityToken, true);
                            logger?.LogDebug("{Name}-{Id} got page token.", Name, Id);

                            if (HasPageItems(pageToken))
                            {
                                logger?.LogDebug("{Name}-{Id} has page entries.", Name, Id);
                                AddTokensToBag(pageToken.GetArray("$.entry", true));
                                logger?.LogDebug("{Name}-{Id} Entry tokens added to bag.", Name, Id);
                                more = HasMore(pageToken);
                                logger?.LogDebug("{Name}-{Id} get more entries {more}.", Name, Id, more);
                            }
                        }
                    }
                    logger?.LogInformation("{Name}-{Id} Building response entries.", Name, Id);
                    JToken lastGoodToken = pageToken ?? resourceEntryToken;
                    lastGoodToken["entry"] = new JArray(tokenBag.ToArray());
                    lastGoodToken["link"] = new JArray();
                    context.ContentString = lastGoodToken.ToString();
                    logger?.LogInformation("{Name}-{Id} Setting response entries.", Name, Id);
                    telemetryClient?.TrackMetric(new MetricTelemetry($"{Name}-{Id}", TimeSpan.FromTicks(DateTime.Now.Ticks - start.Ticks).TotalMilliseconds));
                    return context;
                }
                if (context.Request.Method == HttpMethod.Post && context.ContentString != "")
                {
                    string transformedJson = TransformJson(context.ContentString);
                    byte[] content = Encoding.UTF8.GetBytes(transformedJson);
                    HttpResponseMessage responseMessage = await PostResourceAsync(fpath.Resource, $"_id={fpath.Id}", headers, securityToken, content);
                    if (responseMessage.StatusCode == HttpStatusCode.Created)
                    {
                        context.StatusCode = responseMessage.StatusCode;
                        context.ContentString = responseMessage.Content?.ReadAsStringAsync().Result;
                    }
                }
                if (context.Request.Method == HttpMethod.Put && context.ContentString != "")
                {
                    byte[] content = Encoding.UTF8.GetBytes(context.ContentString);
                    HttpResponseMessage responseMessage = await PutResourceAsync(fpath.Resource, $"_id={fpath.Id}", headers, securityToken, content);
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        context.StatusCode = responseMessage.StatusCode;
                        context.ContentString = responseMessage.Content?.ReadAsStringAsync().Result;
                    }
                }
                if (context.Request.Method == HttpMethod.Delete)
                {
                    HttpResponseMessage responseMessage = await DeleteResourceAsync(fpath.Resource, $"_id={fpath.Id}", headers, securityToken);
                    if (responseMessage.StatusCode == HttpStatusCode.NoContent)
                    {
                        context.StatusCode = responseMessage.StatusCode;
                    }
                }

                return context;

            }
            catch (HttpRequestException httpExp)
            {
                logger?.LogError(httpExp, "{Name}-{Id} filter http fault.", Name, Id);
                context.IsFatal = true;
                context.StatusCode = httpExp.StatusCode ?? HttpStatusCode.InternalServerError;
                OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, Id, true, httpExp, context.StatusCode, null));
                telemetryClient?.TrackMetric(new MetricTelemetry($"{Name}-{Id}-HttpError", TimeSpan.FromTicks(DateTime.Now.Ticks - start.Ticks).TotalMilliseconds));
                return context;
            }
            catch (JPathException jpathExp)
            {
                logger?.LogError(jpathExp, "{Name}-{Id} filter jpath fault.", Name, Id);
                context.IsFatal = true;
                context.StatusCode = HttpStatusCode.BadRequest;
                OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, Id, true, jpathExp, HttpStatusCode.BadRequest, null));
                telemetryClient?.TrackMetric(new MetricTelemetry($"{Name}-{Id}-JPathError", TimeSpan.FromTicks(DateTime.Now.Ticks - start.Ticks).TotalMilliseconds));
                return context;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "{Name}-{Id} filter fault.", Name, Id);
                context.IsFatal = true;
                context.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, Id, true, ex, HttpStatusCode.InternalServerError, null));
                telemetryClient?.TrackMetric(new MetricTelemetry($"{Name}-{Id}-Error", TimeSpan.FromTicks(DateTime.Now.Ticks - start.Ticks).TotalMilliseconds));
                return context;
            }
        }

        private void AddTokensToBag(JArray array)
        {
            if (!array.IsNullOrEmpty())
            {
                tokenBag.AddRange(array.ToArray());
            }
        }
        private static string GetPagePath(JToken token)
        {
            return token.GetValue<string>("$.link[0].url", true);
        }

        private static bool HasPageItems(JToken token)
        {
            return token.GetValue<string>("$.resourceType", true) == "Bundle" && token.GetValue<string>("$.type", true) == "searchset";
        }

        private static bool HasMore(JToken token)
        {
            return token.GetValue<string>("$.link[0].relation", false) == "next";
        }

        private async Task<JToken> GetResourceAsync(string path, string query, NameValueCollection headers, string securityToken, bool throwOnNullContent = true)
        {
            RestRequestBuilder builder = new("GET", fhirServerUrl, securityToken, path, query, headers, null);
            RestRequest request = new(builder, logger);
            async Task<HttpResponseMessage> func()
            {
                return await request.SendAsync();
            }

            var output = Retry.Execute<HttpResponseMessage>(func, TimeSpan.FromSeconds(retryDelaySeconds), maxRetryAttempts, logger);
            var response = output.Result;

            if (response == null)
            {
                throw new HttpRequestException("FHIR server request failed on retry.", null, HttpStatusCode.InternalServerError);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("FHIR server request failed.", null, response.StatusCode);
            }

            if (response.Content.Headers.ContentLength == 0 && throwOnNullContent)
            {
                throw new HttpRequestException("FHIR server return no content.");
            }

            string content = await response.Content?.ReadAsStringAsync();
            return content != null ? JToken.Parse(content) : null;
        }
        private async Task<HttpResponseMessage> PostResourceAsync(string path, string query, NameValueCollection headers, string securityToken, byte[] data, bool throwOnNullContent = true)
        {
            RestRequestBuilder builder = new("POST", fhirServerUrl, securityToken, path, query, headers, data, "application/json");
            RestRequest request = new(builder, logger);
            async Task<HttpResponseMessage> func()
            {
                return await request.SendAsync();
            }

            var output = Retry.Execute<HttpResponseMessage>(func, TimeSpan.FromSeconds(retryDelaySeconds), maxRetryAttempts, logger);
            var response = output.Result;

            if (response == null)
            {
                throw new HttpRequestException("FHIR server request failed on retry.", null, HttpStatusCode.InternalServerError);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("FHIR server request failed.", null, response.StatusCode);
            }

            if (response.Content.Headers.ContentLength == 0 && throwOnNullContent)
            {
                throw new HttpRequestException("FHIR server return no content.");
            }

            return await output;

        }

        private async Task<HttpResponseMessage> PutResourceAsync(string path, string query, NameValueCollection headers, string securityToken, byte[] data, bool throwOnNullContent = true)
        {
            RestRequestBuilder builder = new("PUT", fhirServerUrl, securityToken, path, query, headers, data, "application/json");
            RestRequest request = new(builder, logger);
            async Task<HttpResponseMessage> func()
            {
                return await request.SendAsync();
            }

            var output = Retry.Execute<HttpResponseMessage>(func, TimeSpan.FromSeconds(retryDelaySeconds), maxRetryAttempts, logger);
            var response = output.Result;

            if (response == null)
            {
                throw new HttpRequestException("FHIR server request failed on retry.", null, HttpStatusCode.InternalServerError);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("FHIR server request failed.", null, response.StatusCode);
            }

            if (response.Content.Headers.ContentLength == 0 && throwOnNullContent)
            {
                throw new HttpRequestException("FHIR server return no content.");
            }

            return await output;

        }

        private async Task<HttpResponseMessage> DeleteResourceAsync(string path, string query, NameValueCollection headers, string securityToken, bool throwOnNullContent = true)
        {
            RestRequestBuilder builder = new("Delete", fhirServerUrl, securityToken, path, query, headers, null);
            RestRequest request = new(builder, logger);
            async Task<HttpResponseMessage> func()
            {
                return await request.SendAsync();
            }

            var output = Retry.Execute<HttpResponseMessage>(func, TimeSpan.FromSeconds(retryDelaySeconds), maxRetryAttempts, logger);
            var response = output.Result;

            if (response == null)
            {
                throw new HttpRequestException("FHIR server request failed on retry.", null, HttpStatusCode.InternalServerError);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("FHIR server request failed.", null, response.StatusCode);
            }
            return await output;
        }


        private string TransformJson(string reqContent)
        {
            string json = reqContent;
            JObject jobj = JObject.Parse(json);
            TransformCollection transforms = new();

            if (!jobj.Exists("$.language"))
            {
                AddTransform addTrans = new()
                {
                    JsonPath = "$",
                    AppendNode = "{ \"language\": \"en\" }",
                };
                transforms.Add(addTrans);
            }
            if (!jobj.Exists("$.meta.security"))
            {
                AddTransform addMetaTrans = new()
                {
                    JsonPath = "$",
                    AppendNode = "{\"meta\":{\"security\":[{\"system\":\"http://terminology.hl7.org/CodeSystem/v3-ActReason\",\"code\":\"HTEST\",\"display\":\"test health data\"}]}}"
                };
                transforms.Add(addMetaTrans);
            }

            TransformPolicy policy = new(transforms);
            string transformedJson = policy.Transform(json);
            return transformedJson;
        }
    }
}
