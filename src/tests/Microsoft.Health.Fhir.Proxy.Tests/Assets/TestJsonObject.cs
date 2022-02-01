using System;
using Newtonsoft.Json;

namespace Fhir.Proxy.Tests.Assets
{
    [Serializable]
    [JsonObject]
    public class TestJsonObject
    {
        public TestJsonObject()
        {
        }

        public TestJsonObject(string prop1, string prop2)
        {
            Prop1 = prop1;
            Prop2 = prop2;
        }

        [JsonProperty("prop1")]
        public string Prop1 { get; set; }

        [JsonProperty("prop2")]
        public string Prop2 { get; set; }
    }
}
