using System;
using System.Net.Http;
using System.Threading.Tasks;
using Fhir.Proxy.Bindings;
using Fhir.Proxy.Channels;
using Fhir.Proxy.Filters;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Fhir.Proxy.Pipelines
{
    /// <summary>
    /// A custom operation pipeline for an azure function.
    /// </summary>
    public class AzureFunctionPipeline : IPipeline<HttpRequestData, HttpResponseData>
    {
        /// <summary>
        /// Creates an instance of AzureFunctionPipeline
        /// </summary>
        /// <param name="options">Pipeline options.</param>
        /// <param name="inputFilters">Optional collection of input filters.</param>
        /// <param name="inputChannels">Optional cCollection of input channels.</param>
        /// <param name="binding">Optional binding. </param>
        /// <param name="outputFilters">Optional collection of output filters.</param>
        /// <param name="outputChannels">Optional collection of output channels.</param>
        /// <param name="telemetryClient">Optional application insights telemetry client.</param>
        /// <param name="logger">Optional ILogger.</param>
        public AzureFunctionPipeline(IInputFilterCollection inputFilters = null, IInputChannelCollection inputChannels = null, IBinding binding = null, IOutputFilterCollection outputFilters = null, IOutputChannelCollection outputChannels = null, TelemetryClient telemetryClient = null, ILogger<AzureFunctionPipeline> logger = null)
        {
            id = Guid.NewGuid().ToString();
            pipeline = new WebPipeline(Name, id, inputFilters, inputChannels, binding, outputFilters, outputChannels, telemetryClient, logger);
            pipeline.OnComplete += Pipeline_OnComplete;
            pipeline.OnError += Pipeline_OnError;
        }

        private readonly WebPipeline pipeline;
        private readonly string id;

        /// <summary>
        /// Signals an event that an error occurred in the pipeline.
        /// </summary>
        public event EventHandler<PipelineErrorEventArgs> OnError;

        /// <summary>
        /// Signals an event when the pipeline completes.
        /// </summary>
        public event EventHandler<PipelineCompleteEventArgs> OnComplete;

        /// <summary>
        /// Gets the name of the pipeline.
        /// </summary>
        public string Name { get => "AzureFunctionPipeline"; }

        /// <summary>
        /// Gets the unique ID of the pipeline instance.
        /// </summary>
        public string Id { get => id; }

        /// <summary>
        /// Executes the pipeline and returns a response for the caller.
        /// </summary>
        /// <param name="request">Iniitial request from the Azure Function.</param>
        /// <returns>Response for an Azure Function.</returns>
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
