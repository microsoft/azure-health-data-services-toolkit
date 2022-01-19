using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Proxy.Json.Transforms
{
    /// <summary>
    /// Json transform policy.
    /// </summary>
    [Serializable]
    [JsonObject]
    public class TransformPolicy
    {
        /// <summary>
        /// Creates an instance of TransformPolicy.
        /// </summary>
        public TransformPolicy()
        {
            Transforms = new TransformCollection();
        }

        /// <summary>
        /// Creates an instance of TransformPolicy.
        /// </summary>
        /// <param name="policyId">Unique ID of the policy.</param>
        /// <param name="transforms">Collection of transforms implemented by the policy.</param>
        public TransformPolicy(string policyId, TransformCollection transforms = null)
            : this(transforms)
        {
            PolicyId = policyId;
        }

        /// <summary>
        /// Creates an instance of TransformPolicy.
        /// </summary>
        /// <param name="transforms">Collection of transforms implemented by the policy.</param>
        public TransformPolicy(TransformCollection transforms)
        {
            Transforms = transforms;
        }

        /// <summary>
        /// Gets or sets the ID of the policy.
        /// </summary>
        [JsonProperty("policyId")]
        public string PolicyId { get; set; }

        /// <summary>
        /// Gets or sets a collection of transforms.
        /// </summary>
        [JsonProperty("transforms")]
        public TransformCollection Transforms { get; set; }

        /// <summary>
        /// Transforms a json document and returns the transformed document.
        /// </summary>
        /// <param name="json">Json document to transform.</param>
        /// <returns>Json string as transformed document.</returns>
        public string Transform(string json)
        {
            foreach (var trans in Transforms)
            {
                JObject jobject = trans.Execute(json);
                json = jobject.ToString();
            }

            return json;
        }
    }
}
