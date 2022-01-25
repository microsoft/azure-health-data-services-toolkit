using System.Collections.Generic;
using System.Collections.Specialized;

namespace Microsoft.Health.Fhir.Proxy.Clients.Headers
{
    public interface IHttpCustomHeaderCollection : IList<INameValuePair>
    {
        public NameValueCollection GetHeaders();

        public NameValueCollection AppendHeaders(NameValueCollection items);
    }
}
