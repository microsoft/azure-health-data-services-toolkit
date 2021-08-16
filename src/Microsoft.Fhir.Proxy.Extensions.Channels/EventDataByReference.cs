using Newtonsoft.Json;
using System;

namespace Microsoft.Fhir.Proxy.Extensions.Channels
{
    [Serializable]
    [JsonObject]
    public class EventDataByReference
    {
        public EventDataByReference()
        {
        }

        public EventDataByReference(string container, string blob, string contentType)
        {
            this.Container = container;
            this.Blob = blob;
            this.ContentType = contentType;
        }

        [JsonProperty("container")]
        public string Container { get; set; }

        [JsonProperty("blob")]
        public string Blob { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }
    }
}
