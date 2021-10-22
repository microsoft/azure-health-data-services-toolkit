using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Health.Fhir.Proxy.Json.Transforms
{
    [Serializable]
    [JsonConverter(typeof(TransformConverter))]
    public abstract class Transform
    {
        [JsonProperty("name")]
        public abstract string Name { get; }

        [JsonProperty("jsonPath")]
        public virtual string JsonPath { get; set; }

        public abstract JObject Execute(string json);
    }
}
