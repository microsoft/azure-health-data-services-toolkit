using System;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Fhir.Proxy.Clients
{
    public class RestRequestBuilder
    {
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
        /// <returns></returns>
        public HttpWebRequest Build()
        {
            UriBuilder builder = new(BaseUrl)
            {
                Path = Path,
                Query = QueryString
            };

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(builder.ToString());

            request.Method = Method;

            request.Accept = ContentType;
            request.ContentType = ContentType;

            request.ContentLength = Content == null ? 0 : Content.Length;

            if (!string.IsNullOrEmpty(SecurityToken))
            {
                request.Headers.Add("Authorization", $"Bearer {SecurityToken}");
            }

            if (Certificate != null)
            {
                request.ClientCertificates.Add(Certificate);
            }

            if (Headers != null)
            {
                Headers.Remove("Content-Type");
                Headers.Remove("Content-Length");
                Headers.Remove("Authorization");
                Headers.Remove("Accept");
                request.Headers.Add(Headers);
            }

            return request;
        }
    }
}
