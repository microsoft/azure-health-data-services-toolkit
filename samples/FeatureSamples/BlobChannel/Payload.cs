using Newtonsoft.Json;

namespace BlobChannelSample
{
    [Serializable]
    [JsonObject]
    public class Payload
    {
        [JsonProperty("name")]
        public string Name { get;set; }


        [JsonProperty("value")]
        public string Value { get;set; }
    }
}
