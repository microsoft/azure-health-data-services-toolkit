namespace Fhir.Proxy.Extensions.Channels
{
    /// <summary>
    /// Azure Service Bus channel options used to recieve data.
    /// </summary>
    public class ServiceBusReceiveOptions
    {
        public string ConnectionString { get; set; }

        public string Topic { get; set; }

        public string Subscription { get; set; }

        public string FallbackStorageConnectionString { get; set; }


    }
}
