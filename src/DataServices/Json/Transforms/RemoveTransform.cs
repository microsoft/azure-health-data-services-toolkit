using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Azure.Health.DataServices.Json.Transforms
{
    /// <summary>
    /// Json transform to remove a node.
    /// </summary>
    [Serializable]
    [JsonObject]
    public class RemoveTransform : Transform
    {
        /// <summary>
        /// Gets the name of the transform, i.e., "remove".
        /// </summary>
        [JsonProperty("name")]
        public override string Name => "remove";

        /// <summary>
        /// Executes the remove transform.
        /// </summary>
        /// <param name="json">Json document which to add the node is removed based on the Json path.</param>
        /// <returns>Transformed JObject.</returns>
        public override JObject Execute(string json)
        {
            JObject jobj = JObject.Parse(json);
            JToken? token = jobj.Exists(JsonPath) ? jobj.SelectToken(JsonPath) : null;

            if (token.IsNullOrEmpty())
            {
                return jobj;
            }
            else
            {
                JToken? temp = jobj.SelectToken(JsonPath);
                if (temp.Type == JTokenType.Property)
                {
                    jobj.SelectToken(JsonPath).Remove();
                }

                if (temp.Type == JTokenType.Array)
                {
                    JArray array = (JArray)temp;
                    _ = array.Parent.SelectToken(array.Path);
                    array.RemoveAll();
                    if (array.Parent.Type == JTokenType.Property)
                    {
                        array.Parent.Remove();
                    }
                }
            }

            return jobj;
        }
    }
}
