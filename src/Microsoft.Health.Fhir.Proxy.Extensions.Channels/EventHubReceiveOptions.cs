namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels
{
    /// <summary>
    /// Azure Event Hub channel options used to receive data.
    /// </summary>
    public class EventHubReceiveOptions
    {

        /// <summary>
        /// Gets or sets the event hub connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Azure Event Hub used for this channel. 
        /// </summary>
        public string HubName { get; set; }

        /// <summary>
        /// Gets or sets the Azure Event Hub receiver host storage container.
        /// </summary>
        public string ProcessorStorageContainer { get; set; }

        /// <summary>
        /// Gets or sets an Azure Blob Storage connection string used when data exceeds the allowable Azure Event Hub size.
        /// </summary>
        public string StorageConnectionString { get; set; }
    }
}
