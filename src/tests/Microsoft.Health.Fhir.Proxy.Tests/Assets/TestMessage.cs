using System;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Proxy.Tests.Assets
{
    [Serializable]
    [JsonObject]
    public class TestMessage
    {
        [JsonProperty("value")]
        public string Value { get; set; }

    }
}
