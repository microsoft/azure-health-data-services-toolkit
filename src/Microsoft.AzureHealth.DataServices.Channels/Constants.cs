namespace Microsoft.AzureHealth.DataServices.Channels
{
    /// <summary>
    /// Data services channel constants.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Maximum message length for Service Bus Premium SKU.
        /// </summary>
        public const int ServiceBusPremiumSkuMaxMessageLength = 0xF4240;

        /// <summary>
        /// Maximum message length for Service Bus message when not a Premium SKU.
        /// </summary>
        public const int ServiceBusNonPremiumSkuMaxMessageLength = 0x3E800;

        /// <summary>
        /// Maximum message length for an Event Grid message;
        /// </summary>
        public const int EventGridMaxMessageLength = 1000000;

        /// <summary>
        /// Maximum message length for Event Hub Basic SKU.
        /// </summary>
        public const int EventHubBasicSkuMaxMessageLength = 0x3E800;

        /// <summary>
        /// Maximum message for EVent Hub message when not a Basic SKU.
        /// </summary>
        public const int EventHubNonBasicSkuMaxMessageLength = 0xF4240;

    }
}
