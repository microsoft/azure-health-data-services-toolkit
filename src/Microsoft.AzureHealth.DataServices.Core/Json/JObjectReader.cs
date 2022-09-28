using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureHealth.DataServices.Json
{
    /// <summary>
    /// Abstract JObject reader
    /// </summary>
    public abstract class JObjectReader
    {
        /// <summary>
        /// Creates an instance of JObjectReader.
        /// </summary>
        /// <param name="root">The root of the JSON object.</param>
        protected JObjectReader(JObject root)
        {
            this.root = root;
        }

        /// <summary>
        /// JSON object root.
        /// </summary>
        protected JObject root;

        /// <summary>
        /// Gets an enumerator.
        /// </summary>
        /// <returns>IEnumerator of JToken.</returns>
        public abstract IEnumerator<JToken> GetEnumerator();
    }
}
