using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Azure.Core;
using Microsoft.AspNetCore.Routing;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

namespace Microsoft.AzureHealth.DataServices.Clients
{
    /// <summary>
    /// Builder pattern for REST requests.
    /// </summary>
    public class HttpRequestMessageBuilder
    {
        /// <summary>
        /// Creates an instance of RestRequestBuilder.
        /// </summary>
        /// <param name="method">Http Method.</param>
        /// <param name="baseUrl">Base URL for http request, i.e., scheme and authority.</param>
        /// <param name="securityToken">Security token for http request.</param>
        /// <param name="path">Path of the http request, i.e., scheme://authority/path</param>
        /// <param name="query">Query string for http request.</param>
        /// <param name="headers">Http headers to add to request.</param>
        /// <param name="content">Body content of the http request.</param>
        /// <param name="contentType">Content type of the http request.</param>
        public HttpRequestMessageBuilder(
            HttpMethod method,
            Uri baseUrl,
            string path = null,
            string query = null,
            NameValueCollection headers = null,
            byte[] content = null,
            string securityToken = null,
            string contentType = "application/json")
        {
            _ = method ?? throw new ArgumentNullException(nameof(method));
            _ = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _ = contentType ?? throw new ArgumentNullException(nameof(contentType));

            Method = method;
            BaseUrl = baseUrl;
            Path = path ?? string.Empty;
            QueryString = query ?? string.Empty;
            ContentType = contentType;
            Headers = headers ?? new NameValueCollection();
            Content = content;
            SecurityToken = securityToken;
        }

        /// <summary>
        /// Default User-Agent http header.
        /// </summary>
        public static ProductHeaderValue DefaultUserAgentHeader { get; } = new(
            "Microsoft.AzureHealth.DataServices.Toolkit",
            Assembly.GetExecutingAssembly().GetName().Version.ToString());

        /// <summary>
        /// Gets the base url of the request.
        /// </summary>
        public Uri BaseUrl { get; private set; }

        /// <summary>
        /// Gets the content type of the request.
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// Gets the content of the request.
        /// </summary>
        public byte[] Content { get; private set; }

        /// <summary>
        /// Gets the path of the request.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the query string of the request.
        /// </summary>
        public string QueryString { get; private set; }

        /// <summary>
        /// Gets a collection of HTTP headers of the request.
        /// </summary>
        public NameValueCollection Headers { get; private set; }

        /// <summary>
        /// Gets the HTTP method of the request.
        /// </summary>
        public HttpMethod Method { get; private set; }

        /// <summary>
        /// Gets a security token (JWT) if used in the request.
        /// </summary>
        public string SecurityToken { get; private set; }

        /// <summary>
        /// Builds an HttpRequestMessage.
        /// </summary>
        /// <returns>Http request Message</returns>
        public HttpRequestMessage Build()
        {
            UriBuilder builder = new(BaseUrl)
            {
                Path = Path,
                Query = QueryString,
            };

            string baseUrl = new Uri(builder.ToString()).AbsoluteUri;

            HttpRequestMessage request = new(Method, baseUrl);

            AddPipelineHeadersToRequest(request, Headers);

            if (Content != null)
            {
                request.Content = new ByteArrayContent(Content);

                if (!string.IsNullOrEmpty(ContentType))
                {
                    request.Content.Headers.Remove(HeaderNames.ContentType);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
                }

                request.Content.Headers.ContentLength = Content.Length;
            }

            if (!string.IsNullOrEmpty(SecurityToken))
            {
                request.Headers.Add("Authorization", $"Bearer {SecurityToken}");
            }

            return request;
        }

        private static void AddPipelineHeadersToRequest(HttpRequestMessage request, NameValueCollection headers)
        {
            if (headers?.AllKeys is not null)
            {
                foreach (var item in headers.AllKeys)
                {
                    if (
                        item is not null &&
                        !HttpMessageExtensions.ContentHeaderNames
                            .Any(x => string.Equals(x, item, StringComparison.OrdinalIgnoreCase)))
                    {
                        request.Headers.Add(item, headers.Get(item));
                    }
                }
            }

            request.Headers.Add("Host", request.RequestUri.Authority);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(DefaultUserAgentHeader));
        }
    }
}
