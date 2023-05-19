using System;
using Newtonsoft.Json;

namespace MemoryCacheAndBlobProvider
{
    [Serializable]
    [JsonObject]
    public class TestCacheItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
