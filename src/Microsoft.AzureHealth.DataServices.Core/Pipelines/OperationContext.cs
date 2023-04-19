using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Protocol;

namespace Microsoft.AzureHealth.DataServices.Pipelines
{
    /// <summary>
    /// Operation context used for input and output for a filter in a pipeline.
    /// </summary>
    public class OperationContext
    {
        /// <summary>
        /// Creates an instance of OperationContext
        /// </summary>
        public OperationContext()
        {
            properties = new Dictionary<string, string>();
            headers = new HttpCustomHeaderCollection();
        }

        private readonly Dictionary<string, string> properties;
        private readonly IHttpCustomHeaderCollection headers;

        /// <summary>
        /// Creates an instance of OperationContext
        /// </summary>
        /// <param name="message">Initial http request message</param>
        public OperationContext(HttpRequestMessage message)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));

            Request = message;
            properties = new Dictionary<string, string>();
            this.headers = new HttpCustomHeaderCollection();
            SetContentAsync(message).GetAwaiter();
        }

        /// <summary>
        /// Creates an instance of OperationContext
        /// </summary>
        /// <param name="message">Initial http request message</param>
        /// <param name="headers">Initial header collection</param>
        public OperationContext(HttpRequestMessage message, IHttpCustomHeaderCollection headers)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));

            Request = message;
            properties = new Dictionary<string, string>();
            this.headers = headers;
            SetContentAsync(message).GetAwaiter();
        }

        /// <summary>
        /// Gets a dictionary of custom properties.
        /// </summary>
        public Dictionary<string, string> Properties { get => properties; }

        /// <summary>
        /// Gets or sets an http request.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// Gets or sets the status code for an http response.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the body content of an http request or response.
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets the collection of headers. Request headers feed into bindings. Response headers feed into the output of the pipeline.
        /// </summary>
        public IHttpCustomHeaderCollection Headers { get => headers; }

        /// <summary>
        /// Gets or sets the body content of an http request or response as a string.
        /// </summary>
        public string ContentString
        {
            get => Content != null ? System.Text.Encoding.UTF8.GetString(Content) : null;
            set => Content = value != null ? System.Text.Encoding.UTF8.GetBytes(value) : null;
        }

        /// <summary>
        /// Gets or sets an indicator that processing of the operation context has a fatal error.
        /// </summary>
        public bool IsFatal { get; set; }

        /// <summary>
        /// Gets or sets an exception when a fatal error occurs processing the operation context
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// Updates the request URI and method.
        /// </summary>
        /// <param name="method">HTTP method, e.g., GET, POST, PUT, DELETE</param>
        /// <param name="routePrefix">Route prefix of path, e.g., fhir.</param>
        /// <param name="resource">Optional FHIR resource type.</param>
        /// <param name="id">Optional FHIR Id</param>
        /// <param name="operation">Optional FHIR operation</param>
        /// <param name="version">Optional FHIR version</param>
        public void UpdateFhirRequestUri(HttpMethod method, string routePrefix = null, string resource = null, string id = null, string operation = null, string version = null)
        {
            FhirUriPath requestPath = new(method.ToString(), Request.RequestUri.ToString(), routePrefix);
            requestPath.Resource = resource ?? requestPath.Resource;
            requestPath.Id = id ?? requestPath.Id;
            requestPath.Operation = operation ?? requestPath.Operation;
            requestPath.Version = version ?? requestPath.Version;

            UriBuilder uriBuilder = new(this.Request.RequestUri);
            uriBuilder.Path = requestPath.Path;
            Request.RequestUri = uriBuilder.Uri;

            Request.Method = method;
        }

        /// <summary>
        /// Updates a generic request URI and method.
        /// </summary>
        /// <param name="method">HTTP method, e.g., GET, POST, PUT, DELETE</param>
        /// <param name="baseUrl">Base url of the request exclusive of path or query.</param>
        /// <param name="path">Optional path.</param>
        /// <param name="query">Optional query.</param>
        public void UpdateRequestUri(HttpMethod method, string baseUrl, string path = null, string query = null)
        {
            UriBuilder uriBuilder = new(baseUrl);
            uriBuilder.Path = path;
            uriBuilder.Query = query;
            Request.RequestUri = uriBuilder.Uri;
            Request.Method = method;
        }

        private async Task SetContentAsync(HttpRequestMessage message)
        {
            long? contentLength = message.Content?.Headers.ContentLength;
            if (!(contentLength == null || contentLength == 0))
            {
                Content = await message.Content.ReadAsByteArrayAsync();
            }
        }

        /// <summary>
        /// Create Http Message Request.
        /// </summary>
        /// <returns></returns>
        public HttpMessage ToHttpMessage()
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = ConvertToRequestMethod(context.Request.Method.ToString());

            // #TODO - I think this class is unneeded but we can look later.
            var uri = new RawRequestUriBuilder();
            uri.AppendRaw(_endpoint.ToString(), false);

            if (context.Request.RequestUri.LocalPath.Trim().ToLower() != "/postbundle")
            {
                uri.AppendPath(context.Request.RequestUri.LocalPath.Trim(), escape: false);
            }

            request.Uri = uri;

            NameValueCollection headers = context.Headers.RequestAppendAndReplace(context.Request, false);
            if (headers != null)
            {
                headers.Remove("Content-Type");
                headers.Remove("Content-Length");
                headers.Remove("Authorization");
                headers.Remove("Accept");
                headers.Remove("Host");
                headers.Remove("User-Agent");

                foreach (string item in headers.AllKeys)
                {
                    request.Headers.Add(item, headers.Get(item));
                }
            }

            if (!string.IsNullOrEmpty(context.ContentString))
            {
                request.Content = RequestContent.Create(Encoding.UTF8.GetBytes(context.ContentString));
                request.Headers.Add("Content-Type", context.Request.Content.Headers.ContentType.MediaType.ToString().Trim());
            }
            else
            {
                request.Headers.Add("Content-Type", ContentType);
            }

            request.Headers.Add("Host", new Uri(_endpoint.ToString()).Authority);
            //request.Headers.UserAgent.Add(new ProductInfoHeaderValue(DefaultUserAgentHeader));
            return message;
        }
    }
}
