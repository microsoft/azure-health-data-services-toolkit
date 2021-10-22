using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Health.Fhir.Proxy.Json.Transforms
{
    [Serializable]
    [JsonObject]
    public class TransformPolicy
    {
        public TransformPolicy()
        {
            Transforms = new TransformCollection();
        }

        public TransformPolicy(string policyId, TransformCollection transforms = null)
            : this(transforms)
        {
            PolicyId = policyId;
        }

        public TransformPolicy(TransformCollection transforms)
        {
            Transforms = transforms;
        }

        [JsonProperty("policyId")]
        public string PolicyId { get; set; }

        [JsonProperty("transforms")]
        public TransformCollection Transforms { get; set; }

        public string Transform(string json)
        {
            foreach(var trans in Transforms)
            {
                JObject jobject = trans.Execute(json);
                json = jobject.ToString();
            }

            return json;
        }
    }
}
