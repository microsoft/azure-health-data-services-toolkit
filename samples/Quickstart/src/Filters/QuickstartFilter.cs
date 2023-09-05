using System.Net;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Json;
using Microsoft.AzureHealth.DataServices.Json.Transforms;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Quickstart.Extensions;

namespace Quickstart.Filters
{
    public class QuickstartFilter : IInputFilter
    {
        private readonly string _id;
        private readonly StatusType _status;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger _logger;
        private readonly bool _debug = true;

        public QuickstartFilter(TelemetryClient telemetryClient = null, ILogger<QuickstartFilter> logger = null)
        {
            _id = Guid.NewGuid().ToString();
            _telemetryClient = telemetryClient;
            _logger = logger;
            _status = StatusType.Normal;
        }

        public event EventHandler<FilterErrorEventArgs> OnFilterError;

        public string Id => _id;

        public string Name => "QuickstartFilter";

        public StatusType ExecutionStatusType => _status;

        public Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            DateTime start = DateTime.Now;

            // This filter only corresponds to Put ant Post
            if (context.Request.Method != HttpMethod.Put && context.Request.Method != HttpMethod.Post)
            {
                return Task.FromResult(context);
            }

            try
            {
                JObject jobj = JObject.Parse(context.ContentString);
                TransformCollection transforms = new();
                if (!jobj.Exists("$.communication"))
                {
                    AddTransform addTrans = new()
                    {
                        JsonPath = "$",
                        AppendNode = "{\"communication\":[{\"language\": {\"coding\": [{\"system\":\"urn:ietf:bcp:47\",\"code\": \"en\",\"display\": \"English\"}],\"text\": \"English\"},\"preferred\": true}]}",
                    };
                    transforms.Add(addTrans);
                }

                if (!jobj.Exists("$.meta.security"))
                {
                    AddTransform addMetaTrans = new()
                    {
                        JsonPath = "$",
                        AppendNode = "{\"meta\":{\"security\":[{\"system\":\"http://terminology.hl7.org/CodeSystem/v3-ActReason\",\"code\":\"HTEST\",\"display\":\"test health data\"}]}}",
                    };
                    transforms.Add(addMetaTrans);
                }

                TransformPolicy policy = new(transforms);
                string transformedJson = policy.Transform(context.ContentString);
                context.ContentString = transformedJson;
                return Task.FromResult(context);
            }
            catch (JPathException jpathExp)
            {
                _logger?.LogError(jpathExp, "{Name}-{Id} filter jpath fault.", Name, Id);
                context.IsFatal = true;
                context.StatusCode = HttpStatusCode.BadRequest;
                FilterErrorEventArgs error = new(Name, Id, true, jpathExp, HttpStatusCode.BadRequest, null);
                OnFilterError?.Invoke(this, error);
                _telemetryClient?.TrackMetric(new MetricTelemetry($"{Name}-{Id}-JPathError", TimeSpan.FromTicks(DateTime.Now.Ticks - start.Ticks).TotalMilliseconds));
                return Task.FromResult(context.SetContextErrorBody(error, _debug));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "{Name}-{Id} filter fault.", Name, Id);
                context.IsFatal = true;
                context.StatusCode = HttpStatusCode.InternalServerError;
                FilterErrorEventArgs error = new(Name, Id, true, ex, HttpStatusCode.InternalServerError, null);
                OnFilterError?.Invoke(this, error);
                _telemetryClient?.TrackMetric(new MetricTelemetry($"{Name}-{Id}-Error", TimeSpan.FromTicks(DateTime.Now.Ticks - start.Ticks).TotalMilliseconds));
                return Task.FromResult(context.SetContextErrorBody(error, _debug));
            }
        }
    }
}
