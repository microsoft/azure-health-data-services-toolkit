using Newtonsoft.Json;
using System;

namespace Microsoft.Fhir.Proxy.Extensions.Channels.Configuration
{
    [Serializable]
    public class EventHubSettings
    {
        public EventHubSettings()
        {
            string sku = Environment.GetEnvironmentVariable(Constants.EventHubSku) ?? null;
            _ = Enum.TryParse(sku, out EventHubSkuType skuType);
            EventHubSku = skuType;
            EventHubConnectionString ??= Environment.GetEnvironmentVariable(Constants.EventHubConnectionString) ?? null;
            EventHubName ??= Environment.GetEnvironmentVariable(Constants.EventHubName) ?? null;
            BlobConnectionString ??= Environment.GetEnvironmentVariable(Constants.EventHubStorageConnectionString) ?? null;
            BlobContainer ??= Environment.GetEnvironmentVariable(Constants.EventHubBlobContainerName) ?? null;
            EventHubProcessorContainer ??= Environment.GetEnvironmentVariable(Constants.EventHubProcessorContainerName) ?? null;
        }

        /// <summary>
        /// Creates in instance of EventHubSettings.
        /// </summary>
        /// <param name="eventhubSku">Event Hub SKU used.</param>
        /// <param name="eventhubConnectionString">Connection string for Event Hub.</param>
        /// <param name="eventhubName">Name of Event Hub used.</param>
        /// <param name="blobConnectionString">Connection string to Blob storage.</param>
        /// <param name="blobContainer">Blob container used to output messages that exceed Event Hub size.</param>
        /// <param name="eventhubProcessorContainer">Event Hub processor container in Blob storage used to track Event Hub receive operations.</param>
        public EventHubSettings(EventHubSkuType eventhubSku,
                                string eventhubConnectionString,
                                string eventhubName,
                                string blobConnectionString,
                                string blobContainer,
                                string eventhubProcessorContainer)
        {
            this.EventHubSku = eventhubSku;
            this.EventHubConnectionString = eventhubConnectionString;
            this.EventHubName = eventhubName;
            this.BlobConnectionString = blobConnectionString;
            this.BlobContainer = blobContainer;
            this.EventHubProcessorContainer = eventhubProcessorContainer;
        }

        [JsonProperty("eventhubSku")]
        public EventHubSkuType EventHubSku { get; set; }

        [JsonProperty("eventhubConnectionString")]
        public string EventHubConnectionString { get; set; }

        [JsonProperty("eventhubName")]
        public string EventHubName { get; set; }

        [JsonProperty("blobConnectionString")]
        public string BlobConnectionString { get; set; }

        [JsonProperty("blobContainer")]
        public string BlobContainer { get; set; }

        [JsonProperty("eventhubProcessorContainer")]
        public string EventHubProcessorContainer { get; set; }

    }
}
