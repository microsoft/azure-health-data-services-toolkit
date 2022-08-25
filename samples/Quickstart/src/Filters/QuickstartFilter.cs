using Azure.Health.DataServices.Clients;
using Azure.Health.DataServices.Clients.Headers;
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
        public QuickstartFilter(IHttpCustomHeaderCollection customHeader, TelemetryClient telemetryClient = null, ILogger<QuickstartFilter> logger = null)
        {
            _id = Guid.NewGuid().ToString();
            _telemetryClient = telemetryClient;
            _logger = logger;
            _customHeader = customHeader;
        }

        private readonly string _id;
        private readonly StatusType _status;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger _logger;
        private readonly IHttpCustomHeaderCollection _customHeader;

        public string Id => _id;
        public string Name => "QuickstartFilter";
        public StatusType ExecutionStatusType => _status;
        public event EventHandler<FilterErrorEventArgs> OnFilterError;

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            DateTime start = DateTime.Now;
            _customHeader.AppendAndReplace(context.Request);
            // This filter only corresponds to Put ant Post
            if (context.Request.Method != HttpMethod.Put && context.Request.Method != HttpMethod.Post)
                return context;


            try
            {
                JObject jobj = JObject.Parse(context.ContentString);
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
                string transformedJson = policy.Transform(context.ContentString);
                var content = new StringContent(transformedJson, Encoding.UTF8, "application/json");
                context.Request.Content = content;

                return context;

            }
            catch (JPathException jpathExp)
            {
                _logger?.LogError(jpathExp, "{Name}-{Id} filter jpath fault.", Name, Id);
                context.IsFatal = true;
                context.StatusCode = HttpStatusCode.BadRequest;
                OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, Id, true, jpathExp, HttpStatusCode.BadRequest, null));
                _telemetryClient?.TrackMetric(new MetricTelemetry($"{Name}-{Id}-JPathError", TimeSpan.FromTicks(DateTime.Now.Ticks - start.Ticks).TotalMilliseconds));
                return context;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "{Name}-{Id} filter fault.", Name, Id);
                context.IsFatal = true;
                context.StatusCode = HttpStatusCode.InternalServerError;
                OnFilterError?.Invoke(this, new FilterErrorEventArgs(Name, Id, true, ex, HttpStatusCode.InternalServerError, null));
                _telemetryClient?.TrackMetric(new MetricTelemetry($"{Name}-{Id}-Error", TimeSpan.FromTicks(DateTime.Now.Ticks - start.Ticks).TotalMilliseconds));
                return context;
            }
        }
    }
}
