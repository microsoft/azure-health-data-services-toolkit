namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels
{
    public class ServiceBusOptions
    {
        public ServiceBusSkuType Sku { get; set; }

        public string ConnectionString { get; set; }

        public string Topic { get; set; }

        public string FallbackStorageConnectionString { get; set; }

        public string FallbackStorageContainer { get; set; }

        public string Subscription { get; set; }
    }
}
