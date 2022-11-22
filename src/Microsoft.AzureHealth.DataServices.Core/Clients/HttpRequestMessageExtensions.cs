using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AzureHealth.DataServices.Clients
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
        public static NameValueCollection GetHeaders(this HttpRequestMessage request, bool restricted = true) =>
            GetHeaders(request.Headers, restricted);

        /// <summary>
        /// Converts HttpRequestMessage headers into a NameValueCollection.
        /// </summary>
        /// <param name="response">Response message.</param>
        /// <param name="restricted">If true (default), omits the following headers, Content-Type, Content-Length, Authorization, Accept, Host, User-Agent.  Otherwise, returns all headers. </param>
        /// <returns>NameValueCollection of http headers.</returns>
        public static NameValueCollection GetHeaders(this HttpResponseMessage response, bool restricted = true) =>
            GetHeaders(response.Headers, restricted);


        private static NameValueCollection GetHeaders(HttpHeaders genericHeaderList, bool restricted)
        {
            NameValueCollection nvc = new();

            foreach (var header in genericHeaderList)
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

        private static readonly List<string> _contentHeaderNames = new List<string>
        {
            HeaderNames.Accept,
            HeaderNames.ContentDisposition,
            HeaderNames.ContentEncoding,
            HeaderNames.ContentLanguage,
            HeaderNames.ContentLength,
            HeaderNames.ContentLocation,
            HeaderNames.ContentRange,
            HeaderNames.ContentType,
            HeaderNames.Expires,
            HeaderNames.LastModified,
        };

        /// <summary>
        /// Adds custom headers to a HttpResponseMessage object
        /// </summary>
        /// <param name="response">Response data to modify.</param>
        /// <param name="headers">Custom headers collection.</param>
        public static void AddCustomHeadersToResponse(this HttpResponseMessage response, IHttpCustomHeaderCollection headers)
        {
            foreach (var header in headers.Where(x => x.HeaderType == Clients.Headers.CustomHeaderType.ResponseStatic))
            {
                if (_contentHeaderNames.Any(x => x.ToLowerInvariant() == header.Name.ToLowerInvariant()))
                {
                    response.Content.Headers.Add(header.Name, header.Value);
                }
                else if (!header.Name.Equals("Server", StringComparison.InvariantCultureIgnoreCase))
                {
                    response.Headers.Add(header.Name, header.Value);
                }
            }
        }
    }
}
