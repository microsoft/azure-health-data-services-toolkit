namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels
{
    public class EventHubReceiveOptions
    {
        public string ConnectionString { get; set; }

        public string HubName { get; set; }

        public string ProcessorStorageContainer { get; set; }

        public string StorageConnectionString { get; set; }
    }
}
