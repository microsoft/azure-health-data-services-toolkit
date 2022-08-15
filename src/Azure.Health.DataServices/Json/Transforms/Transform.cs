using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Azure.Health.DataServices.Json.Transforms
{
    /// <summary>
    /// Abstract transform.
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(TransformConverter))]
    public abstract class Transform
    {
        /// <summary>
        /// Gets the name of the type of transform.
        /// </summary>
        [JsonProperty("name")]
        public abstract string Name { get; }

        /// <summary>
        /// Gets or sets the Json path needed to apply the transform to the input json document.
        /// </summary>
        [JsonProperty("jsonPath")]
        public virtual string JsonPath { get; set; }

        /// <summary>
        /// Executes the transform.
        /// </summary>
        /// <param name="json">Document json to apply the transform and json path.</param>
        /// <returns>Transform json as JObject.</returns>
        public abstract JObject Execute(string json);
    }
}
