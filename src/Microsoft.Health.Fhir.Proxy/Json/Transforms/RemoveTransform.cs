using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Health.Fhir.Proxy.Json.Transforms
{
    [Serializable]
    [JsonObject]
    public class RemoveTransform : Transform
    {
        [JsonProperty("name")]
        public override string Name => "remove";

        public override JObject Execute(string json)
        {
            JObject jobj = JObject.Parse(json);
            JToken? token = jobj.Exists(JsonPath) ? jobj.SelectToken(JsonPath) : null;

            if(token.IsNullOrEmpty())
            {
                return jobj;
            }
            else
            {
                JToken? temp = jobj.SelectToken(JsonPath);
                if(temp.Type == JTokenType.Property)
                {
                    jobj.SelectToken(JsonPath).Remove();
                }

                if(temp.Type == JTokenType.Array)
                {
                    JArray array = (JArray)temp;
                    JToken? t = array.Parent.SelectToken(array.Path);
                    array.RemoveAll();
                    if(array.Parent.Type == JTokenType.Property)
                    {
                        array.Parent.Remove();
                    }
                }
            }

            return jobj;
        }
    }
}
