namespace Microsoft.Health.Fhir.Proxy.Caching.StorageProviders
{
    public class AzureBlobStorageCacheOptions
    {
        public string ConnectionString { get; set; }

        public string Container { get; set; }
    }
}
