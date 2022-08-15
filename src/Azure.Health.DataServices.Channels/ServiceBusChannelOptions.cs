using Azure.Health.DataServices.Pipelines;

namespace Azure.Health.DataServices.Channels
{
    /// <summary>
    /// Azure Service Bus channel options used to send data.
    /// </summary>
    public class ServiceBusChannelOptions
    {
        /// <summary>
        /// Gets or sets the type of service bus sku used.
        /// </summary>
        public ServiceBusSkuType Sku { get; set; }

        /// <summary>
        /// Gets or sets the service bus connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the service bus topic.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Gets or sets the service bus subscription used to receive events.
        /// </summary>
        /// <remarks>Not used when only sending.</remarks>
        public string Subscription { get; set; }

        /// <summary>
        /// Gets or sets the service bus queue for sending or receiving.
        /// </summary>
        public string Queue { get; set; }

        /// <summary>
        /// Gets or sets the status type.
        /// </summary>
        public StatusType ExecutionStatusType { get; set; }

        /// <summary>
        /// Gets or sets the fallback storage for messages of excessive size.
        /// </summary>
        public string FallbackStorageConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the fallback storage container for messages of excessive size.
        /// </summary>
        public string FallbackStorageContainer { get; set; }
    }
}
