using System;
using Microsoft.AzureHealth.DataServices.Channels;
using Newtonsoft.Json;

namespace Microsoft.AzureHealth.DataServices.Tests.Configuration
{
    /// <summary>
    /// Configuration for Event Hub channel.
    /// </summary>
    [Serializable]
    [JsonObject]
    public class EventHubConfig
    {
        /// <summary>
        /// Creates an instance of EventHubConfig.
        /// </summary>
        public EventHubConfig()
        {
        }

        /// <summary>
        /// Creates in instance of EventHubConfig.
        /// </summary>
        /// <param name="eventhubSku">Event Hub SKU used.</param>
        /// <param name="eventhubConnectionString">Connection string for Event Hub.</param>
        /// <param name="eventhubName">Name of Event Hub used.</param>
        /// <param name="blobConnectionString">Connection string to Blob storage.</param>
        /// <param name="blobStorageAccountName">name string to Blob storage account.</param>
        /// <param name="blobContainer">Blob container used to output messages that exceed Event Hub size.</param>
        /// <param name="eventhubProcessorContainer">Event Hub processor container in Blob storage used to track Event Hub receive operations.</param>
        public EventHubConfig(
            EventHubSkuType eventhubSku,
            string eventhubConnectionString,
            string eventhubName,
            string blobConnectionString,
            string blobContainer,
            string blobStorageAccountName,
            string eventhubProcessorContainer)
        {
            this.EventHubSku = eventhubSku;
            this.EventHubConnectionString = eventhubConnectionString;
            this.EventHubName = eventhubName;
            this.EventHubBlobConnectionString = blobConnectionString;
            this.EventHubBlobContainer = blobContainer;
            this.EventHubProcessorContainer = eventhubProcessorContainer;
            this.EventHubBlobStorageAccountName = blobStorageAccountName;
        }

        /// <summary>
        /// Gets or sets Event Hub SKU.
        /// </summary>
        [JsonProperty("eventhubSku")]
        public EventHubSkuType EventHubSku { get; set; }

        /// <summary>
        /// Gets or sets Event Hub connection string.
        /// </summary>
        [JsonProperty("eventhubConnectionString")]
        public string EventHubConnectionString { get; set; }

        /// <summary>
        /// Gets or sets Event Hub name.
        /// </summary>
        [JsonProperty("eventhubName")]
        public string EventHubName { get; set; }

        /// <summary>
        /// Gets or sets Event Hub namespace.
        /// </summary>
        [JsonProperty("eventhubNamespace")]
        public string EventHubNamespace { get; set; }

        /// <summary>
        /// Gets or sets Azure storage connection string for managing large files.
        /// </summary>
        [JsonProperty("eventHubBlobConnectionString")]
        public string EventHubBlobConnectionString { get; set; }

        /// <summary>
        /// Gets or sets Azure storage account name for managing large files.
        /// </summary>
        [JsonProperty("eventHubBlobStorageAccountName")]
        public string EventHubBlobStorageAccountName { get; set; }

        /// <summary>
        /// Gets or sets blob storage container name for managing large files.
        /// </summary>
        [JsonProperty("eventHubBlobContainer")]
        public string EventHubBlobContainer { get; set; }

        /// <summary>
        /// Gets or sets Event Hub processor container name in blob storage.
        /// </summary>
        /// <remarks>Used only when reading an event hub.</remarks>
        [JsonProperty("eventhubProcessorContainer")]
        public string EventHubProcessorContainer { get; set; }
    }
}
