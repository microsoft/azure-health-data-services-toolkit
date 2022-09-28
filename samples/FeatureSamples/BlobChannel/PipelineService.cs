using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Logging;

namespace BlobChannelSample
{
    public class PipelineService : IPipelineService
    {
        public PipelineService(IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline, ILogger<PipelineService> logger = null)
        {
            this.pipeline = pipeline;
            this.logger = logger;
        }

        private readonly IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline;
        private readonly ILogger logger;

        public async Task ExecuteAsync(HttpRequestMessage message)
        {
            logger?.LogInformation("Start pipeline");
            await pipeline.ExecuteAsync(message);
        }
    }
}
