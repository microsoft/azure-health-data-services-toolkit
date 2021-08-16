using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Extensions.Channels.Configuration
{
    [Serializable]
    [JsonObject]
    public class ServiceBusSettings
    {
        public ServiceBusSettings()
        {

        }

        [JsonProperty("servicebusConnectionString")]
        public string ServiceBusConnectionString { get; set; }

        [JsonProperty("servicebusSku")]
        public ServiceBusSkuType ServiceBusSku { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("subscription")]
        public string Subscription { get; set; }

        [JsonProperty("blobConnectionString")]
        public string BlobConnectionString { get; set; }

        [JsonProperty("servicebusBlobContainer")]
        public string ServiceBusBlobContainer { get; set; }
    }
}
