using DataServices.Pipelines;

namespace DataServices.Channels
{
    /// <summary>
    /// Azure Event Hub channel options used to send data.
    /// </summary>
    public class EventHubChannelOptions
    {
        /// <summary>
        /// Gets or sets the Azure Event Hub Sku used to determine the maximum message size allowed by the Event Hub.
        /// </summary>
        public EventHubSkuType Sku { get; set; }

        /// <summary>
        /// Gets or sets the Azure Event Hub connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Azure Event Hub used for this channel. 
        /// </summary>
        public string HubName { get; set; }

        /// <summary>
        /// Gets or sets the requirement for execution of the channel.
        /// </summary>
        public StatusType ExecutionStatusType { get; set; }

        /// <summary>
        /// Gets or sets an Azure Blob Storage connection string used when data exceeds the allowable Azure Event Hub size.
        /// </summary>
        public string FallbackStorageConnectionString { get; set; }

        /// <summary>
        /// Gets or sets an Azure Blob Storage container used to store data when data exceeds the allowable Azure Event Hub size.
        /// </summary>
        public string FallbackStorageContainer { get; set; }

        /// <summary>
        /// Gets or sets the Azure Event Hub receiver host storage container.
        /// </summary>
        /// <remarks>Used only for receiving from Event Hub.</remarks>
        public string ProcessorStorageContainer { get; set; }




    }
}
