using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Microsoft.Health.Fhir.Proxy.Json.Transforms
{
    [Serializable]
    [JsonObject]
    public class AddTransform : Transform
    {
        [JsonProperty("name")]
        public override string Name => "add";

        [JsonProperty("appendNode")]
        public string AppendNode { get; set; }

        public override JObject Execute(string json)
        {
            JToken appendNode = JToken.Parse(AppendNode);
            JObject jobj = JObject.Parse(json);
            JToken? token = jobj.Exists(JsonPath) ? jobj.SelectToken(JsonPath) : null;

            if (token.IsNullOrEmpty())
            {
                return jobj;
            }

            if (token.IsArray())
            {
                jobj.SelectToken(JsonPath).Last().AddAfterSelf(AppendNode);
            }
            else
            {
                foreach (var childProp in appendNode.Children())
                {
                    if (childProp is JProperty prop)
                    {
                        jobj.SelectToken(JsonPath)[prop.Name] = prop.Value;
                    }
                }
            }

            return jobj;
        }
    }
}
