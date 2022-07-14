using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DataServices.Json
{
    /// <summary>
    /// FHIR bundle reader.
    /// </summary>
    public class BundleReader : JObjectReader
    {
        /// <summary>
        /// Creates a new instance of BundleReader.
        /// </summary>
        /// <param name="root">The root object to read.</param>
        /// <param name="ifNoneExist">FHIR ifNoneExists flag omits if false.</param>
        public BundleReader(JObject root, bool ifNoneExist)
            : base(root)
        {
            this.ifNoneExist = ifNoneExist;
        }

        private readonly bool ifNoneExist;

        /// <summary>
        /// Gets the bundle enumerator.
        /// </summary>
        /// <returns>Enumerator of JToken if exists, otherwise null.</returns>
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
