namespace Microsoft.Health.Fhir.Proxy.Caching.StorageProviders
{
    /// <summary>
    /// Options for Azure Blob Storage cache backing store.
    /// </summary>
    public class AzureBlobStorageCacheOptions
    {
        public string ConnectionString { get; set; }

        public string Container { get; set; }
    }
}
