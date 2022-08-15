using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Health.DataServices.Protocol;

namespace Azure.Health.DataServices.Pipelines
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
        }

        private readonly Dictionary<string, string> properties;

        /// <summary>
        /// Creates an instance of OperationContext
        /// </summary>
        /// <param name="message">Initial http request message</param>
        public OperationContext(HttpRequestMessage message)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));

            Request = message;
            properties = new Dictionary<string, string>();
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
    }
}
