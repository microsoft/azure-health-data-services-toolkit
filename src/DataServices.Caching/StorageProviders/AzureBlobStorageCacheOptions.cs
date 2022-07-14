namespace DataServices.Caching.StorageProviders
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
        /// Gets or sets the Azure Blob service endpoint used when you choose to use MSI versus a connection string.
        /// </summary>
        public string BlobServiceEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the Azure Blob Storage container for the data.
        /// </summary>
        public string Container { get; set; }



    }
}
