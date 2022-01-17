using Microsoft.Health.Fhir.Proxy.Pipelines;

namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels
{
    /// <summary>
    /// Azure Service Bus channel options used to send data.
    /// </summary>
    public class ServiceBusSendOptions
    {
        public ServiceBusSkuType Sku { get; set; }

        public string ConnectionString { get; set; }

        public string Topic { get; set; }

        public StatusType ExecutionStatusType { get; set; }

        public string FallbackStorageConnectionString { get; set; }

        public string FallbackStorageContainer { get; set; }
    }
}
