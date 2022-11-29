using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;

namespace Microsoft.AzureHealth.DataServices.Clients.Headers
{
    /// <summary>
    /// Interface that defines how headers from a request can be obtained from the collection of custom headers.
    /// </summary>
    public interface IHttpCustomHeaderCollection : IList<IHeaderNameValuePair>
    {
        /// <summary>
        /// Appends and replaces existing request headers with custom headers and returns the modified collection headers.
        /// </summary>
        /// <param name="request">Http request message.</param>
        /// <param name="restricted">Restrict to used editable headers</param>
        /// <returns>Modified collection headers</returns>
        NameValueCollection RequestAppendAndReplace(HttpRequestMessage request, bool restricted = true);

        /// <summary>
        /// Updates this header collection from a HttpResponseMessage
        /// </summary>
        /// <param name="request">Http request message.</param>
        /// <param name="restricted">If true (default), omits the following headers, Content-Length, Authorization, Transfer-Encoding.  Otherwise, returns all headers. </param>
        /// <returns>Modified collection headers</returns>
        void UpdateFromResponse(HttpResponseMessage request, bool restricted = true);
    }
}
