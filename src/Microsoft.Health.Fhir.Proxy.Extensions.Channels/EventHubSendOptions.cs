using Microsoft.Health.Fhir.Proxy.Pipelines;

namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels
{
    /// <summary>
    /// Azure Event Hub channel options used to send data.
    /// </summary>
    public class EventHubSendOptions
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



    }
}
