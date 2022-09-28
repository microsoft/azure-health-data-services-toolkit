using Azure.Messaging.EventGrid;
using Azure.Storage.Queues.Models;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AzureHealth.DataServices.Storage;
using Microsoft.Extensions.Logging;
using System.Text;

namespace EventGridChannelSample
{
    public class PipelineService : IPipelineService
    {
        public PipelineService(IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline, AzureQueueConfig config, ILogger<PipelineService> logger = null)
        {
            this.pipeline = pipeline;
            this.config = config;
            this.logger = logger;
            queue = new StorageQueue(config.ConnectionString, logger);
        }

        private readonly IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline;
        private readonly ILogger logger;
        private readonly AzureQueueConfig config;
        private readonly StorageQueue queue;

        public event EventHandler<ReceiveEventArgs> OnReceive;

        public async Task ExecuteAsync(HttpRequestMessage message)
        {
            logger?.LogInformation("Starting pipeline");
            await pipeline.ExecuteAsync(message);

            QueueMessage queueMessage = null;
            while (queueMessage == null)
            {
                queueMessage = await queue.DequeueAsync(config.QueueName, TimeSpan.FromSeconds(20.0), default);
                
                if (queueMessage != null)
                {
                    string text = queueMessage.MessageText;
                    EventGridEvent gridEvent = EventGridEvent.Parse(new BinaryData(Convert.FromBase64String(text)));
                    OnReceive?.Invoke(this, new ReceiveEventArgs(gridEvent));
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}
