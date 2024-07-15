using System.Net;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Json;
using Microsoft.AzureHealth.DataServices.Json.Transforms;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UseCaseSample.Extensions;

namespace UseCaseSample.Filters
{
    public class UseCaseSampleFilter : IOutputFilter
    {
        private readonly string _id;
        private readonly StatusType _status;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger _logger;
        private readonly bool _debug = true;

        public UseCaseSampleFilter(TelemetryClient telemetryClient = null, ILogger<UseCaseSampleFilter> logger = null)
        {
            _id = Guid.NewGuid().ToString();
            _telemetryClient = telemetryClient;
            _logger = logger;
            _status = StatusType.Normal;
        }

        public event EventHandler<FilterErrorEventArgs> OnFilterError;

        public string Id => _id;

        public string Name => "UseCaseSampleFilter";

        public StatusType ExecutionStatusType => _status;

        public Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            DateTime start = DateTime.Now;
            try
            {
                string url = context.Request.RequestUri.ToString();
                Uri uri = new Uri(url);
                bool containsMetadata = uri.AbsolutePath.Contains("metadata", StringComparison.OrdinalIgnoreCase);
                if (containsMetadata)
                {
                    JObject jobj = JObject.Parse(context.ContentString);
                    TransformCollection transforms = new();
                    if (!jobj.Exists("$.instantiates"))
                    {
                        AddTransform addTrans = new()
                        {
                            JsonPath = "$",
                            AppendNode = "{\"instantiates\":[\"http://hl7.org/fhir/us/core/CapabilityStatement/us-core-server\"]}",
                        };
                        transforms.Add(addTrans);
                    }

                    TransformPolicy policy = new(transforms);
                    string transformedJson = policy.Transform(context.ContentString);
                    context.ContentString = transformedJson;
                }

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
