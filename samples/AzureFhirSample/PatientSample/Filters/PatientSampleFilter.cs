using CustomHeader;
using Azure.Health.DataServices.Clients;
using Azure.Health.DataServices.Clients.Headers;
using Azure.Health.DataServices.Filters;
using Azure.Health.DataServices.Json;
using Azure.Health.DataServices.Pipelines;
using Azure.Health.DataServices.Protocol;
using Azure.Health.DataServices.Security;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PatientSample.Filters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Health.DataServices.Json.Transforms;
using System.Text;

namespace PatientSample.Filters
{
    public class PatientSampleFilter : IInputFilter
    {
        public PatientSampleFilter(IOptions<PatientSampleOptions> options, IAuthenticator authenticator, ICustomHeaderService customHeader, TelemetryClient telemetryClient = null, ILogger<PatientSampleFilter> logger = null)
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
            var headers = context.Request.GetHeaders();

            context.Request.Headers.Add("X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST", "QuickstartCustomOperation");
            var customHeaders = customHeader.GetHeaders(context.Request);

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
                }
                else if (context.Request.Method == HttpMethod.Post)
                {
                    string content = context.Request.Content.ReadAsStringAsync().Result;
                    JToken resourceToken = await PostResourceAsync(fpath.Resource, $"_id={fpath.Id}", headers, securityToken, content);
                    logger?.LogInformation("{Name}-{Id} Setting response entries.", Name, Id);
                    telemetryClient?.TrackMetric(new MetricTelemetry($"{Name}-{Id}", TimeSpan.FromTicks(DateTime.Now.Ticks - start.Ticks).TotalMilliseconds));
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

        private async Task<JToken> PostResourceAsync(string path, string query, NameValueCollection headers, string securityToken, string data, bool throwOnNullContent = true)
        {
            RestRequestBuilder builder = new("Post", fhirServerUrl, securityToken, path, query, headers, TransFormJson(data));
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

            if (response.Content.Headers.ContentLength == 0)
            {
                throw new HttpRequestException("FHIR server return no content.");
            }

            string content = await response.Content?.ReadAsStringAsync();
            return content != null ? JToken.Parse(content) : null;

        }

        private byte[] TransFormJson(string body)
        {            
            JObject jobj = JObject.Parse(body);
            TransformCollection transforms = new();

            #region Add Language
            if (!jobj.Exists("$.language"))
            {
                AddTransform addTrans = new()
                {
                    JsonPath = "$",
                    AppendNode = "{ \"language\": \"en\"}",
                };
                transforms.Add(addTrans);
            }


            #endregion

            #region Security
            if (!jobj.Exists("$.meta.security"))
            {
                AddTransform addMetaTrans = new()
                {
                    JsonPath = "$",
                    AppendNode = "{\"meta\":{\"security\":[{\"system\":\"http://terminology.hl7.org/CodeSystem/v3-ActReason\",\"code\":\"HTEST\",\"display\":\"test health data\"}]}}"
                };


                transforms.Add(addMetaTrans);
            }
            #endregion

            string transformedJson = "";
            if (transforms.Count > 0)
            {
                TransformPolicy policy = new(transforms);
                transformedJson = policy.Transform(body);
            }
            return Encoding.UTF8.GetBytes(transformedJson);
        }
    }
}
