namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels
{
    public class BlobStorageSendOptions
    {
        public string ConnectionString { get; set; }

        public string Container { get; set; }

        public long? InitialTransferSize { get; set; }

        public int? MaxConcurrency { get; set; }

        public int? MaxTransferSize { get; set; }
    }
}
