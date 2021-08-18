using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Microsoft.Fhir.Proxy.Json
{
    public abstract class JObjectReader
    {
        protected JObjectReader(JObject root)
        {
            this.root = root;
        }

        protected JObject root;

        public abstract IEnumerator<JToken> GetEnumerator();
    }
}
