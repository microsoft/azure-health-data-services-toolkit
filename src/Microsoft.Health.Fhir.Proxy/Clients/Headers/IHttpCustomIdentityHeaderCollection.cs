using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using Microsoft.Azure.Functions.Worker.Http;

namespace Microsoft.Health.Fhir.Proxy.Clients.Headers
{
    public interface IHttpCustomIdentityHeaderCollection : IList<IClaimValuePair>
    {
        NameValueCollection AppendCustomHeaders(HttpRequestData request, NameValueCollection headers);

        NameValueCollection AppendCustomHeaders(HttpRequestMessage request, NameValueCollection headers);

    }
}
