using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Proxy.Bindings;
using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Filters;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    public class AzureFunctionPipeline : IPipeline<HttpRequestData, HttpResponseData>
    {
        public AzureFunctionPipeline(IOptions<PipelineOptions> options, IInputFilterCollection inputFilters = null, IInputChannelCollection inputChannels = null, IBinding binding = null, IOutputFilterCollection outputFilters = null, IOutputChannelCollection outputChannels = null, TelemetryClient client = null, ILogger logger = null)
        {
            id = Guid.NewGuid().ToString();
            pipeline = new WebPipeline(Name, id, options, inputFilters, inputChannels, binding, outputFilters,  outputChannels, client, logger);
            pipeline.OnComplete += Pipeline_OnComplete;
            pipeline.OnError += Pipeline_OnError;
        }

        private readonly WebPipeline pipeline;
        private readonly string id;

        public event EventHandler<PipelineErrorEventArgs> OnError;
        public event EventHandler<PipelineCompleteEventArgs> OnComplete;

        public string Name { get => "AzureFunctionPipeline"; }

        public string Id { get => id; }

        public async Task<HttpResponseData> ExecuteAsync(HttpRequestData request)
        {
            HttpRequestMessage message = request.ConvertToHttpRequestMesssage();
            HttpResponseMessage response = await pipeline.ExecuteAsync(message);
            return await response.ConvertToHttpResponseDataAsync(request);
        }

        private void Pipeline_OnError(object sender, PipelineErrorEventArgs e)
        {
            OnError?.Invoke(this, e);
        }

        private void Pipeline_OnComplete(object sender, PipelineCompleteEventArgs e)
        {
            OnComplete?.Invoke(this, e);
        }

    }
}
