using Newtonsoft.Json;
using System;

namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels.Configuration
{
    [Serializable]
    [JsonObject]
    public class ServiceBusConfig
    {
        public ServiceBusConfig()
        {
        }

        [JsonProperty("servicebusConnectionString")]
        public string ServiceBusConnectionString { get; set; }

        [JsonProperty("servicebusSku")]
        public ServiceBusSkuType ServiceBusSku { get; set; }

        [JsonProperty("serviceBustopic")]
        public string ServiceBusTopic { get; set; }

        [JsonProperty("serviceBusSubscription")]
        public string ServiceBusSubscription { get; set; }

        [JsonProperty("serviceBusBlobConnectionString")]
        public string ServiceBusBlobConnectionString { get; set; }

        [JsonProperty("serviceBusBlobContainer")]
        public string ServiceBusBlobContainer { get; set; }
    }
}
