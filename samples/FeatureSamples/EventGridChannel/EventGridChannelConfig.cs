
using Microsoft.AzureHealth.DataServices.Pipelines;

namespace EventGridChannelSample
{
    public class EventGridChannelConfig
    {
        public string TopicUriString { get; set; }

        public string Subject { get; set; }

        public string AccessKey { get; set; }

        public StatusType ExecutionStatusType { get; set; }

        public string DataVersion { get; set; }

        public string EventType { get; set; }

        public string QueueName { get; set; }

        public string FallbackStorageConnectionString { get; set; }

        public string FallbackStorageContainer { get; set; }

    }
}
