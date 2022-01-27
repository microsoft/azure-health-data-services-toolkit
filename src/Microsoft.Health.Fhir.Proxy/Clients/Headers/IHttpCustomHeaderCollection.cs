using System.Collections.Generic;
using System.Collections.Specialized;

namespace Microsoft.Health.Fhir.Proxy.Clients.Headers
{
    /// <summary>
    /// Interface that defines how headers can be obtained from the collection.
    /// </summary>
    public interface IHttpCustomHeaderCollection : IList<INameValuePair>
    {
        /// <summary>
        /// Gets a collection of headers stored in the collection.
        /// </summary>
        /// <returns>Headers in the collection.</returns>
        public NameValueCollection GetHeaders();

        /// <summary>
        /// Appends headers to an existing collection of headers and returns results.
        /// </summary>
        /// <param name="items">Existing header collection.</param>
        /// <returns>The existing headers with the headers in the collection appended.</returns>
        public NameValueCollection AppendHeaders(NameValueCollection items);
    }
}
