using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fhir.Proxy.Json.Transforms
{
    /// <summary>
    /// Json replace transform.
    /// </summary>
    [Serializable]
    [JsonObject]
    public class ReplaceTransform : Transform
    {
        /// <summary>
        /// Gets the name of the transform, i.e., "replace".
        /// </summary>
        [JsonProperty("name")]
        public override string Name => "replace";

        /// <summary>
        /// Gets or sets the json node to replace.
        /// </summary>
        [JsonProperty("replaceNode")]
        public string ReplaceNode { get; set; }

        /// <summary>
        /// Executes the replace transform.
        /// </summary>
        /// <param name="json">Json document which a node is replaced based on the Json path and replace node.</param>
        /// <returns></returns>
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
