namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels
{
    public class EventHubSendOptions
    {
        public EventHubSkuType Sku { get; set; }

        public string ConnectionString { get; set; }

        public string HubName { get; set; }

        public string ProcessorContainer { get; set; }

        public string FallbackStorageConnectionString { get; set; }

        public string FallbackStorageContainer { get; set; }



    }
}
