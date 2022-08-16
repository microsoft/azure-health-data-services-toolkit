using Newtonsoft.Json;
using System;

namespace MemoryCacheAndRedisProvider
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
