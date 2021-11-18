using System;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Health.Fhir.Proxy.Clients
{
    /// <summary>
    /// Builder pattern for REST requests.
    /// </summary>
    public class RestRequestBuilder
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
        public RestRequestBuilder(string method,
                                  string baseUrl,
                                  string securityToken,
                                  string? path,
                                  string? query,
                                  NameValueCollection? headers,
                                  byte[]? content,
                                  string contentType = "application/json")
        {
            _ = method ?? throw new ArgumentNullException(nameof(method));
            _ = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _ = contentType ?? throw new ArgumentNullException(nameof(contentType));
            _ = securityToken ?? throw new ArgumentNullException(nameof(securityToken));

            Method = method;
            BaseUrl = baseUrl;
            Path = path;
            QueryString = query;
            ContentType = contentType;
            Headers = headers;
            Content = content;
            SecurityToken = securityToken;
        }

        /// <summary>
        /// Creates an instance of RestRequestBuilder.
        /// </summary>
        /// <param name="method">Http Method.</param>
        /// <param name="baseUrl">Base URL for http request, i.e., scheme and authority.</param>
        /// <param name="certificate">X509 certificate to use as a security token.</param>
        /// <param name="path">Path of the http request, i.e., scheme://authority/path</param>
        /// <param name="query">Query string for http request.</param>
        /// <param name="headers">Http headers to add to request.</param>
        /// <param name="content">Body content of the http request.</param>
        /// <param name="contentType">Content type of the http request.</param>
        public RestRequestBuilder(string method,
                                  string baseUrl,
                                  X509Certificate2 certificate,
                                  string? path,
                                  string? query,
                                  NameValueCollection? headers,
                                  byte[]? content,
                                  string contentType = "application/json")
        {
            _ = method ?? throw new ArgumentNullException(nameof(method));
            _ = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _ = contentType ?? throw new ArgumentNullException(nameof(contentType));
            _ = certificate ?? throw new ArgumentNullException(nameof(certificate));

            Method = method;
            BaseUrl = baseUrl;
            Path = path;
            QueryString = query;
            ContentType = contentType;
            Headers = headers;
            Content = content;
            Certificate = certificate;
        }

        /// <summary>
        /// Gets the base url of the request.
        /// </summary>
        public string BaseUrl { get; private set; }

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
        public string Method { get; private set; }

        /// <summary>
        /// Gets a security token (JWT) if used in the request.
        /// </summary>
        public string SecurityToken { get; private set; }

        /// <summary>
        /// Get a client certificate if used in the request.
        /// </summary>
        public X509Certificate2 Certificate { get; internal set; }

        /// <summary>
        /// Builds an Http Web Request.
        /// </summary>
        /// <returns>HttpWebRequest</returns>
        public HttpWebRequest Build()
        {
            UriBuilder builder = new(BaseUrl)
            {
                Path = Path,
                Query = QueryString
            };

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(builder.ToString());

            if (Headers != null)
            {
                Headers.Remove("Content-Type");
                Headers.Remove("Content-Length");
                Headers.Remove("Authorization");
                Headers.Remove("Accept");
                Headers.Remove("Host");
                Headers.Add("Host", new Uri(BaseUrl).Authority);
                request.Headers.Add(Headers);
            }

            request.Method = Method;

            request.Accept = ContentType;

            if (Content != null)
            {
                request.ContentType = ContentType;
                request.ContentLength = Content.Length;
            }

            if (!string.IsNullOrEmpty(SecurityToken))
            {
                request.Headers.Add("Authorization", $"Bearer {SecurityToken}");
            }

            if (Certificate != null)
            {
                request.ClientCertificates.Add(Certificate);
            }

            return request;
        }
    }
}
