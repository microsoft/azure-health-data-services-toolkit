using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fhir.Proxy.Json.Transforms
{
    /// <summary>
    /// Json transform to add a node.
    /// </summary>
    [Serializable]
    [JsonObject]
    public class AddTransform : Transform
    {
        /// <summary>
        /// Gets the name of the transform, i.e., "add".
        /// </summary>
        [JsonProperty("name")]
        public override string Name => "add";

        /// <summary>
        /// Gets or sets the json on the node to add.
        /// </summary>
        [JsonProperty("appendNode")]
        public string AppendNode { get; set; }

        /// <summary>
        /// Executes the add transform.
        /// </summary>
        /// <param name="json">Json document which to add the node based on the Json path.</param>
        /// <returns></returns>
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
