using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Fhir.Proxy.Bindings;
using Microsoft.Fhir.Proxy.Configuration;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Pipelines
{
    public sealed class AzureFunctionPipelineManager : IPipelineManager<HttpRequestData, HttpResponseData>
    {
        public AzureFunctionPipelineManager(PipelineSettings input, PipelineBinding binding, PipelineSettings output, TelemetryClient client = null, ILogger logger = null)
        {
            this.input = input;
            this.binding = binding;
            this.output = output;
            this.client = client;
            this.logger = logger;
        }

        private readonly PipelineSettings input;
        private readonly PipelineBinding binding;
        private readonly PipelineSettings output;
        private readonly TelemetryClient client;
        private readonly ILogger logger;

        public Func<OperationContext, OperationContext> BeforeInput { get; set; }

        public Func<OperationContext, OperationContext> AfterInput { get; set; }

        public Func<OperationContext, OperationContext> BeforeOutput { get; set; }

        public Func<OperationContext, OperationContext> AfterOutput { get; set; }

        public async Task<HttpResponseData> ExecuteAsync(HttpRequestData request)
        {
            long startTicks = DateTime.Now.Ticks;

            try
            {
                HttpRequestMessage message = request.ConvertToHttpRequestMesssage();
                OperationContext context = new(message);

                logger?.LogTrace($"Can BeforeInput {BeforeInput != null}");
                context = BeforeInput != null ? BeforeInput(context) : context;
                logger?.LogTrace($"Input pipeline present{input != null}");
                context = await RunPipelineAsync(input, context);
                logger?.LogTrace($"Can AfterInput {AfterInput != null}");
                context = AfterInput != null ? AfterInput(context) : context;
                logger?.LogTrace($"Binding present {binding != null}");
                context = await binding.ExecuteAsync(context);
                logger?.LogTrace($"Can BeforeOutput {BeforeOutput != null}");
                context = BeforeOutput != null ? BeforeOutput(context) : context;
                logger?.LogTrace($"Output pipeline present {output != null}");
                context = await RunPipelineAsync(output, context);
                logger?.LogTrace($"Can AfterOutput {AfterOutput != null}");
                context = AfterOutput != null ? AfterOutput(context) : context;
                logger?.LogTrace($"Context is fatal {context.IsFatal}");
                logger?.LogTrace($"Context is initial status code {context.StatusCode}");
                context.StatusCode = !context.IsFatal && context.StatusCode == 0 ? HttpStatusCode.OK : context.StatusCode;
                logger?.LogTrace($"Context is final status code {context.StatusCode}");
                HttpResponseData response = await context.ConvertToHttpResponseData(request);
                logger?.LogTrace($"Pipelines complete.");
                client?.TrackMetric(new MetricTelemetry("Pipeline ExecutionTime", TimeSpan.FromTicks(DateTime.Now.Ticks - startTicks).TotalMilliseconds));
                return response;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Pipeline error with fault response.");
                client?.TrackMetric(new MetricTelemetry("Pipeline execution fault", TimeSpan.FromTicks(DateTime.Now.Ticks - startTicks).TotalMilliseconds));
            }

            logger?.LogTrace("Fault executing pipelines returning 503 for response.");
            return request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        private async Task<OperationContext> RunPipelineAsync(PipelineSettings settings, OperationContext context)
        {
            if (settings == null)
            {
                logger?.LogTrace("Pipeline settings omitted, echoing context.");
                return context;
            }

            PipelineBuilder builder = new(settings);
            Pipeline pipeline = builder.Build();
            logger?.LogTrace($"Pipeline {pipeline.Name} - {pipeline.Id} built.");

            pipeline.OnError += (_, arg) =>
            {
                logger?.LogError(arg.Error, $"{pipeline.Name} - {pipeline.Id} pipeline fault.");
                client?.TrackException(new ExceptionTelemetry(arg.Error) { Message = "Pipeline fault." });
                pipeline.Dispose();
                throw arg.Error;
            };

            logger?.LogTrace($"Executing pipeline {pipeline.Name} - {pipeline.Id}");
            return await pipeline.ExecuteAsync(context);
        }
    }
}
