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
            Root = root;
        }

        protected JObject Root { get; set; }

        /// <summary>
        /// Gets an enumerator.
        /// </summary>
        /// <returns>IEnumerator of JToken.</returns>
        public abstract IEnumerator<JToken> GetEnumerator();
    }
}
