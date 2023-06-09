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
    public static class HttpMessageExtensions
    {
        internal static readonly IReadOnlyCollection<string> RequestRestrictedHeaderList = new List<string>
        {
            HeaderNames.ContentType,
            HeaderNames.ContentLength,
            HeaderNames.Authorization,
            HeaderNames.Host,
            HeaderNames.UserAgent,
        };

        internal static readonly IReadOnlyCollection<string> ResponseRestrictedHeaderList = new List<string>
        {
             HeaderNames.ContentLength,
             HeaderNames.Host,
             HeaderNames.TransferEncoding,
        };

        internal static readonly IReadOnlyCollection<string> ContentHeaderNames = new List<string>
        {
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
        /// Converts HttpRequestMessage headers into a NameValueCollection.
        /// </summary>
        /// <param name="request">Request message.</param>
        /// <param name="restricted">If true (default), omits protected headers.</param>
        /// <returns>NameValueCollection of http headers.</returns>
        public static NameValueCollection GetHeaders(this HttpRequestMessage request, bool restricted = true) =>
            GetHeaders(request.Headers, restricted ? RequestRestrictedHeaderList : Array.Empty<string>());

        /// <summary>
        /// Converts HttpRequestMessage content headers into a NameValueCollection.
        /// </summary>
        /// <param name="request">Request message.</param>
        /// <param name="restricted">If true (default), omits protected headers.</param>
        /// <returns>NameValueCollection of http content headers.</returns>
        public static NameValueCollection GetContentHeaders(this HttpRequestMessage request, bool restricted = true) =>
            GetHeaders(request.Content.Headers, restricted ? RequestRestrictedHeaderList : Array.Empty<string>());

        /// <summary>
        /// Converts HttpRequestMessage headers into a NameValueCollection.
        /// </summary>
        /// <param name="response">Response message.</param>
        /// <param name="restricted">If true (default), omits protected headers.</param>
        /// <returns>NameValueCollection of http headers.</returns>
        public static NameValueCollection GetHeaders(this HttpResponseMessage response, bool restricted = true) =>
            GetHeaders(response.Headers, restricted ? ResponseRestrictedHeaderList : Array.Empty<string>());

        /// <summary>
        /// Converts HttpRequestMessage content headers into a NameValueCollection.
        /// </summary>
        /// <param name="response">Response message.</param>
        /// <param name="restricted">If true (default), omits protected headers. </param>
        /// <returns>NameValueCollection of http headers.</returns>
        public static NameValueCollection GetContentHeaders(this HttpResponseMessage response, bool restricted = true) =>
            GetHeaders(response.Content.Headers, restricted ? ResponseRestrictedHeaderList : Array.Empty<string>());

        /// <summary>
        /// Adds custom headers to a HttpResponseMessage object
        /// </summary>
        /// <param name="response">Response data to modify.</param>
        /// <param name="headers">Custom headers collection.</param>
        public static void AddCustomHeadersToResponse(this HttpResponseMessage response, IHttpCustomHeaderCollection headers)
        {
            foreach (IHeaderNameValuePair header in headers.Where(x => x.HeaderType == CustomHeaderType.ResponseStatic))
            {
                if (ContentHeaderNames.Any(x => string.Equals(x, header.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    response.Content.Headers.TryAddWithoutValidation(header.Name, header.Value);
                }
                else
                {
                    response.Headers.TryAddWithoutValidation(header.Name, header.Value);
                }
            }
        }

        private static NameValueCollection GetHeaders(HttpHeaders genericHeaderList, IReadOnlyCollection<string> restrictedHeaderList)
        {
            NameValueCollection nvc = new();

            foreach (KeyValuePair<string, IEnumerable<string>> header in genericHeaderList)
            {
                if (!restrictedHeaderList.Any(x => string.Equals(x, header.Key, StringComparison.OrdinalIgnoreCase)))
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
