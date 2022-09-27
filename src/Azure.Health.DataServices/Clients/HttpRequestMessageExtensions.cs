using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;

namespace Azure.Health.DataServices.Clients
{
    /// <summary>
    /// Extensions for HttpRequestMessage.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        private static string[] RestrictedHeaderList = new string[] { "content-type", "content-length", "authorization", "accept", "host", "user-agent" };


        /// <summary>
        /// Converts HttpRequestMessage headers into a NameValueCollection.
        /// </summary>
        /// <param name="request">Request message.</param>
        /// <param name="restricted">If true (default), omits the following headers, Content-Type, Content-Length, Authorization, Accept, Host, User-Agent.  Otherwise, returns all headers. </param>
        /// <returns>NameValueCollection of http headers.</returns>
        public static NameValueCollection GetHeaders(this HttpRequestMessage request, bool restricted = true)
        {
            NameValueCollection nvc = new();
            foreach (var header in request.Headers)
            {
                if (!(restricted && RestrictedHeaderList.Contains(header.Key.ToLowerInvariant())))
                {
                    foreach (var val in header.Value)
                    {

                        nvc.Add(header.Key, val);
                    }
                }
            }

            return nvc;
        }

    }
}
