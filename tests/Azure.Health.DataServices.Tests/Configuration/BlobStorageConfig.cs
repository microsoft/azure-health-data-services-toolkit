using Newtonsoft.Json;

namespace Azure.Health.DataServices.Tests.Configuration
{
    /// <summary>
    /// Configuration of Azure blob storage channel.
    /// </summary>
    public class BlobStorageConfig
    {
        /// <summary>
        /// Creates an instance of BlobStorageConfig
        /// </summary>
        public BlobStorageConfig()
        {
        }

        /// <summary>
        /// Creates an instance of BlobStorageConfig.
        /// </summary>
        /// <param name="connectionString">Azure storage connection string.</param>
        /// <param name="container">Blob container where files will be stored.</param>
        /// <param name="initialTransferSize">Optional initial transfer size in bytes.</param>
        /// <param name="maxConcurrency">Optional maximum concurrency.</param>
        /// <param name="maxTransferSize">Optional maximum transfer size in bytes.</param>
        public BlobStorageConfig(string connectionString, string container,
                                 long? initialTransferSize = null,
                                 int? maxConcurrency = null,
                                 int? maxTransferSize = null)
        {
            BlobStorageChannelConnectionString = connectionString;
            BlobStorageChannelContainer = container;
            InitialTransferSize = initialTransferSize;
            MaxConcurrency = maxConcurrency;
            MaxTransferSize = maxTransferSize;
        }

        /// <summary>
        /// Gets or sets the Azure storage connection string.
        /// </summary>
        [JsonProperty("blobStorageChannelConnectionString")]
        public string BlobStorageChannelConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the blob storage container name where files are stored.
        /// </summary>
        [JsonProperty("blobStorageChannelContainer")]
        public string BlobStorageChannelContainer { get; set; }

        /// <summary>
        /// Gets or sets the optional initial transfer size in bytes.
        /// </summary>
        [JsonProperty("initialTransferSize")]
        public long? InitialTransferSize { get; set; }

        /// <summary>
        /// Gets or sets the optional maximum concurrency size.
        /// </summary>
        [JsonProperty("maxConcurrency")]
        public int? MaxConcurrency { get; set; }

        /// <summary>
        /// Gets or sets the optional maximum transfer size in bytes.
        /// </summary>
        [JsonProperty("maxTransferSize")]
        public int? MaxTransferSize { get; set; }

    }
}
