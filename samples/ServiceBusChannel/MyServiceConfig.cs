

using Azure.Health.DataServices.Channels;
using Azure.Health.DataServices.Pipelines;

namespace ServiceBusChannelSample
{
    public class MyServiceConfig
    {

        public string ConnectionString { get; set; }

        public string Topic { get; set; }

        public string Subscription { get; set; }

        public StatusType ExecutionStatusType { get; set; }  

        public ServiceBusSkuType Sku { get; set; }

        public string FallbackStorageConnectionString { get; set; }

        public string FallbackStorageContainer { get; set; }

    }
}
