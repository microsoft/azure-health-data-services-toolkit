using System;
using Newtonsoft.Json;

namespace Azure.Health.DataServices.Tests.Assets
{
    [Serializable]
    [JsonObject]
    public class TestMessage
    {
        [JsonProperty("value")]
        public string Value { get; set; }

    }
}
