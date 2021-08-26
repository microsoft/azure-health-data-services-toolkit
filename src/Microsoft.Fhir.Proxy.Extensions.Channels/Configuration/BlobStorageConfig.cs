using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels.Configuration
{
    public class BlobStorageConfig
    {
        public BlobStorageConfig()
        {
        }

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

        [JsonProperty("blobStorageChannelConnectionString")]
        public string BlobStorageChannelConnectionString { get; set; }

        [JsonProperty("blobStorageChannelContainer")]
        public string BlobStorageChannelContainer { get; set; }

        [JsonProperty("initialTransferSize")]
        public long? InitialTransferSize { get; set; }

        [JsonProperty("maxConcurrency")]
        public int? MaxConcurrency { get; set; }

        [JsonProperty("maxTransferSize")]
        public int? MaxTransferSize { get; set; }

    }
}
