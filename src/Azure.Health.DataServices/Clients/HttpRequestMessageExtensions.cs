using System.Collections.Specialized;
using System.Net.Http;

namespace Azure.Health.DataServices.Clients
{
    /// <summary>
    /// Extensions for HttpRequestMessage.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Converts HttpRequestMessage headers into a NameValueCollection.
        /// </summary>
        /// <param name="request">Request message.</param>
        /// <returns>NameValueCollection of http headers.</returns>
        public static NameValueCollection GetHeaders(this HttpRequestMessage request)
        {
            NameValueCollection nvc = new();
            foreach (var header in request.Headers)
            {
                foreach (var val in header.Value)
                {
                    nvc.Add(header.Key, val);
                }
            }

            return nvc;
        }
    }
}
