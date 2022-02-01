using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using Microsoft.Azure.Functions.Worker.Http;

namespace Fhir.Proxy.Clients.Headers
{
    /// <summary>
    /// Interface that defines how headers can be obtained from the collection.
    /// </summary>
    public interface IHttpCustomIdentityHeaderCollection : IList<IClaimValuePair>
    {
        /// <summary>
        /// Appends custom headers by matching claim types found in security token of the Authorization header of the request.
        /// </summary>
        /// <param name="request">Azure function request containing Authorization header.</param>
        /// <param name="headers">Existing header colleection.</param>
        /// <returns>Appended custom identity headers to the existing header collection.</returns>
        NameValueCollection AppendCustomHeaders(HttpRequestData request, NameValueCollection headers);

        /// <summary>
        /// Appends custom headers by matching claim types found in security token of the Authorization header of the request.
        /// </summary>
        /// <param name="request">Http request containing Authorization header.</param>
        /// <param name="headers">Existing header colleection.</param>
        /// <returns>Appended custom identity headers to the existing header collection.</returns>

        NameValueCollection AppendCustomHeaders(HttpRequestMessage request, NameValueCollection headers);

    }
}
