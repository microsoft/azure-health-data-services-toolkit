using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Logging;

namespace BlobChannelSample
{
    public class PipelineService : IPipelineService
    {
        private readonly IPipeline<HttpRequestMessage, HttpResponseMessage> _pipeline;
        private readonly ILogger _logger;

        public PipelineService(IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline, ILogger<PipelineService> logger = null)
        {
            _pipeline = pipeline;
            _logger = logger;
        }

        public async Task ExecuteAsync(HttpRequestMessage message)
        {
            _logger?.LogInformation("Start pipeline");
            await _pipeline.ExecuteAsync(message);
        }
    }
}
