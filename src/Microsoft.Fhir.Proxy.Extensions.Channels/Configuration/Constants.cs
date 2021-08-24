using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Extensions.Channels.Configuration
{
    public class Constants
    {
        public const string EventHubConnectionString = "PROXY_EVENTHUB_CONNECTIONSTRING";
        public const string EventHubName = "PROXY_EVENTHUB_NAME";
        public const string EventHubStorageConnectionString = "PROXY_EVENTHUB_STORAGE_CONNECTIONSTRING";
        public const string EventHubBlobContainerName = "PROXY_EVENTHUB_BLOBCONTAINER_NAME";
        public const string EventHubSku = "PROXY_EVENTHUB_SKU";
        public const string EventHubProcessorContainerName = "PROXY_EVENTHUB_PROCESSORCONTAINER_NAME";
        public const string ServiceBusConnectionString = "PROXY_SERVICEBUS_CONNECTIONSTRING";
        public const string ServiceBusTopic = "PROXY_SERVICEBUS_TOPIC";
        public const string ServiceBusSubscription = "PROXY_SERVICEBUS_SUBSCRIPTION";
        public const string ServiceBusSku = "PROXY_SERVICEBUS_SKU";
        public const string ServiceBusBlobContainerName = "PROXY_SERVICEBUS_BLOBCONTAINER_NAME";
        public const string ServiceBusStorageConnectionString = "PROXY_SERVICEBUS_STORAGE_CONNECTIONSTRING";

    }
}
