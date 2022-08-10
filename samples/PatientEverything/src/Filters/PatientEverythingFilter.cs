using DataServices.Clients;
using DataServices.Filters;
using DataServices.Json;
using DataServices.Pipelines;
using DataServices.Protocol;
using DataServices.Security;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PatientEverything.Filters
{
    public class PatientEverythingFilter : IInputFilter
    {
        public PatientEverythingFilter(IOptions<PatientEverythingOptions> options, IAuthenticator authenticator, TelemetryClient telemetryClient = null, ILogger<PatientEverythingFilter> logger = null)
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
        public string Id => id;

        public string Name => "PatientEverythingFilter";

        public StatusType ExecutionStatusType => status;

        public event EventHandler<FilterErrorEventArgs> OnFilterError;

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            DateTime start = DateTime.Now;
            if (context == null || context.IsFatal)
            {
                return context;
            }

            string securityToken = await authenticator.AquireTokenForClientAsync(fhirServerUrl);
            JToken pageToken = null;
            FhirUriPath fpath = new(context.Request.Method.ToString(), context.Request.RequestUri.ToString());
            var headers = context.Request.GetHeaders();
            try
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
    }
}
