using Newtonsoft.Json;

namespace Shared
{
    [Serializable]
    [JsonObject]
    public class Message
    {
        [JsonProperty("value")]
        public string Value { get; set; }

    }
}