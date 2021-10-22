using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Health.Fhir.Proxy.Json.Transforms
{
    [Serializable]
    [JsonObject]
    public class ReplaceTransform : Transform
    {
        [JsonProperty("name")]
        public override string Name => "replace";

        [JsonProperty("replaceNode")]
        public string ReplaceNode { get; set; }

        public override JObject Execute(string json)
        {
            JToken replaceNode = JToken.Parse(ReplaceNode);
            JObject jobj = JObject.Parse(json);
            JToken? token = jobj.Exists(JsonPath) ? jobj.SelectToken(JsonPath) : null;

            if (token.IsNullOrEmpty())
            {
                return jobj;
            }

            jobj.SelectToken(JsonPath).Replace(replaceNode);

            return jobj;
        }
    }
}
