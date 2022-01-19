using System;
using Microsoft.Health.Fhir.Proxy.Extensions.Channels;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Proxy.Tests.Configuration
{
    /// <summary>
    /// Configuration for Service Bus channel.
    /// </summary>
    [Serializable]
    [JsonObject]
    public class ServiceBusConfig
    {
        /// <summary>
        /// Creates an instance of ServiceBusConfig.
        /// </summary>
        public ServiceBusConfig()
        {
        }

        /// <summary>
        /// Gets or sets Service Bus connection string.
        /// </summary>
        [JsonProperty("servicebusConnectionString")]
        public string ServiceBusConnectionString { get; set; }

        /// <summary>
        /// Gets or sets Service Bus SKU.
        /// </summary>
        [JsonProperty("servicebusSku")]
        public ServiceBusSkuType ServiceBusSku { get; set; }

        /// <summary>
        /// Gets or sets Service Bus topic.
        /// </summary>
        [JsonProperty("serviceBustopic")]
        public string ServiceBusTopic { get; set; }

        /// <summary>
        /// Gets or sets Service Bus subscription.
        /// </summary>
        /// <remarks>Used only when reading from Service Bus.</remarks>
        [JsonProperty("serviceBusSubscription")]
        public string ServiceBusSubscription { get; set; }

        /// <summary>
        /// Gets or sets Azure storage connection string for managing large files.
        /// </summary>
        [JsonProperty("serviceBusBlobConnectionString")]
        public string ServiceBusBlobConnectionString { get; set; }

        /// <summary>
        /// Gets or sets blob storage container name for managing large files.
        /// </summary>
        [JsonProperty("serviceBusBlobContainer")]
        public string ServiceBusBlobContainer { get; set; }
    }
}
