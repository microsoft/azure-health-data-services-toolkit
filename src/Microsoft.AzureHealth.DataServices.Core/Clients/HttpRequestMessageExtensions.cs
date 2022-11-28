using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Core;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AzureHealth.DataServices.Clients
{
    /// <summary>
    /// Extensions for HttpRequestMessage.
    /// </summary>
    public static class HttpMessageExtensions
    {
        private static string[] RequestRestrictedHeaderList = new string[] { "content-type", "content-length", "authorization", "accept", "host", "user-agent" };
        
        private static string[] ResponseRestrictedHeaderList = new string[] { "content-length", "host", "transfer-encoding" };


        /// <summary>
        /// Converts HttpRequestMessage headers into a NameValueCollection.
        /// </summary>
        /// <param name="request">Request message.</param>
        /// <param name="restricted">If true (default), omits the following headers, Content-Type, Content-Length, Authorization, Accept, Host, User-Agent.  Otherwise, returns all headers. </param>
        /// <returns>NameValueCollection of http headers.</returns>
        public static NameValueCollection GetHeaders(this HttpRequestMessage request, bool restricted = true)
        {
            if (restricted)
            {
                return GetHeaders(request.Headers, RequestRestrictedHeaderList);
            }
            else
            {
                return GetHeaders(request.Headers, Array.Empty<string>());
            }
        }

        /// <summary>
        /// Converts HttpRequestMessage headers into a NameValueCollection.
        /// </summary>
        /// <param name="response">Response message.</param>
        /// <param name="restricted">If true (default), omits the following headers, Content-Length, Authorization, Transfer-Encoding.  Otherwise, returns all headers. </param>
        /// <returns>NameValueCollection of http headers.</returns>
        public static NameValueCollection GetHeaders(this HttpResponseMessage response, bool restricted = true)
        {
            NameValueCollection headers = new();
            NameValueCollection contentHeaders = new();

            if (restricted)
            {
                headers = GetHeaders(response.Headers, ResponseRestrictedHeaderList);
                contentHeaders = GetHeaders(response.Content.Headers, ResponseRestrictedHeaderList);
            }
            else
            {
                headers = GetHeaders(response.Headers, Array.Empty<string>());
                contentHeaders = GetHeaders(response.Content.Headers, ResponseRestrictedHeaderList);
            }

            foreach (string key in contentHeaders.AllKeys)
            {
                headers.Add(key, contentHeaders[key]);
            }

            return headers;
        }
        
        private static NameValueCollection GetHeaders(HttpHeaders genericHeaderList, string[] restrictedHeaderList)
        {
            NameValueCollection nvc = new();

            foreach (var header in genericHeaderList)
            {
                if (!restrictedHeaderList.Contains(header.Key.ToLowerInvariant()))
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
                    // remove existing headers
                    var existingHeaderNames = response.Content.Headers.Where(x => x.Key.ToLowerInvariant() == header.Name.ToLowerInvariant()).Select(x => x.Key);
                    foreach (var name in existingHeaderNames)
                    {
                        response.Content.Headers.Remove(name);
                    }

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
