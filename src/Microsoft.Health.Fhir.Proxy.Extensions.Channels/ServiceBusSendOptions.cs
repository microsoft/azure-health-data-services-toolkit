namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels
{
    public class ServiceBusSendOptions
    {
        public ServiceBusSkuType Sku { get; set; }

        public string ConnectionString { get; set; }

        public string Topic { get; set; }

        public string FallbackStorageConnectionString { get; set; }

        public string FallbackStorageContainer { get; set; }
    }
}
