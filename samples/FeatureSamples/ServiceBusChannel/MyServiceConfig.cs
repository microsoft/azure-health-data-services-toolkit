

using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Pipelines;

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
