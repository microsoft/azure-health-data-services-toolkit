using Microsoft.AzureHealth.DataServices.Pipelines;

namespace Microsoft.AzureHealth.DataServices.Channels
{
    /// <summary>
    /// Azure Event Grid channel options used to send data.
    /// </summary>
    public class EventGridChannelOptions
    {
        /// <summary>
        /// Gets or sets the Azure Event Grid topic.
        /// </summary>
        public string TopicUriString { get; set; }

        /// <summary>
        /// Gets or sets the Azure Event Grid access key.
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets Azure Event Grid subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the Azure Event Grid event type.
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets the Azure Event Grid data version.
        /// </summary>
        public string DataVersion { get; set; }

        /// <summary>
        /// Gets or sets the requirement for execution of the channel.
        /// </summary>
        public StatusType ExecutionStatusType { get; set; }

        /// <summary>
        /// Gets or sets an Azure Blob Storage connection string used when data exceeds the allowable Azure Event Grid size.
        /// </summary>
        public string FallbackStorageConnectionString { get; set; }

        /// <summary>
        /// Gets or sets an Azure Blob Storage container used to store data when data exceeds the allowable Azure Event Grid size.
        /// </summary>
        public string FallbackStorageContainer { get; set; }
    }
}
