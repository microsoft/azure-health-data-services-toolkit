
using Azure.Health.DataServices.Channels;
using Azure.Health.DataServices.Pipelines;

namespace EventHubChannelSample
{
    public class MyServiceConfig
    {
        public EventHubSkuType Sku { get; set; }   
        
        public string ConnectionString { get; set; }

        public string HubName { get; set; }

        public string FallbackStorageConnectionString { get; set; }

        public string FallbackStorageContainer { get; set; }

        public string ProcessorStorageContainer { get; set; }

        public StatusType ExecutionStatusType { get; set; }




    }
}
