using Newtonsoft.Json;
using System;

namespace Microsoft.Fhir.Proxy.Extensions.Channels.Configuration
{
    [Serializable]
    [JsonObject]
    public class ServiceBusSettings
    {
        public ServiceBusSettings()
        {
            string sku = Environment.GetEnvironmentVariable(Constants.ServiceBusSku) ?? null;
            _ = Enum.TryParse(sku, out ServiceBusSkuType skuType);
            ServiceBusSku = skuType;
            ServiceBusConnectionString ??= Environment.GetEnvironmentVariable(Constants.ServiceBusConnectionString) ?? null;
            ServiceBusTopic ??= Environment.GetEnvironmentVariable(Constants.ServiceBusTopic) ?? null;
            ServiceBusSubscription ??= Environment.GetEnvironmentVariable(Constants.ServiceBusSubscription) ?? null;
            ServiceBusBlobConnectionString ??= Environment.GetEnvironmentVariable(Constants.ServiceBusStorageConnectionString) ?? null;
            ServiceBusBlobContainer ??= Environment.GetEnvironmentVariable(Constants.ServiceBusBlobContainerName);
        }

        [JsonProperty("servicebusConnectionString")]
        public string ServiceBusConnectionString { get; set; }

        [JsonProperty("servicebusSku")]
        public ServiceBusSkuType ServiceBusSku { get; set; }

        [JsonProperty("topic")]
        public string ServiceBusTopic { get; set; }

        [JsonProperty("subscription")]
        public string ServiceBusSubscription { get; set; }

        [JsonProperty("blobConnectionString")]
        public string ServiceBusBlobConnectionString { get; set; }

        [JsonProperty("servicebusBlobContainer")]
        public string ServiceBusBlobContainer { get; set; }
    }
}
