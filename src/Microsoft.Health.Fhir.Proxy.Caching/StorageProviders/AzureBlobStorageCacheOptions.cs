namespace Microsoft.Health.Fhir.Proxy.Caching.StorageProviders
{
    /// <summary>
    /// Options for Azure Blob Storage cache backing store.
    /// </summary>
    public class AzureBlobStorageCacheOptions
    {
        /// <summary>
        /// Gets or sets the Azure Blob Storage connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Azure Blob Storage container for the data.
        /// </summary>
        public string Container { get; set; }
    }
}
