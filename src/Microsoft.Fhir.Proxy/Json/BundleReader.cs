using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Proxy.Json
{
    public class BundleReader : JObjectReader
    {
        public BundleReader(JObject root, bool ifNoneExist)
            : base(root)
        {
            this.ifNoneExist = ifNoneExist;
        }

        private readonly bool ifNoneExist;

        public override IEnumerator<JToken> GetEnumerator()
        {
            if (root.IsArray("$.entry"))
            {
                JArray entries = (JArray)root["entry"];
                return new BundleEnumerator(entries, ifNoneExist);
            }
            else
            {
                return null;
            }
        }
    }
}
