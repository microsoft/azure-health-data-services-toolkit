using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Fhir.Proxy.Json
{
    /// <summary>
    /// Abstract JObject reader
    /// </summary>
    public abstract class JObjectReader
    {
        protected JObjectReader(JObject root)
        {
            this.root = root;
        }

        protected JObject root;

        /// <summary>
        /// Gets an enumerator.
        /// </summary>
        /// <returns>IEnumerator of JToken.</returns>
        public abstract IEnumerator<JToken> GetEnumerator();
    }
}
