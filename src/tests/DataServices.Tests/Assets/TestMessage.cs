using System;
using Newtonsoft.Json;

namespace DataServices.Tests.Assets
{
    [Serializable]
    [JsonObject]
    public class TestMessage
    {
        [JsonProperty("value")]
        public string Value { get; set; }

    }
}
