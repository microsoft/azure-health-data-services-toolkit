using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Proxy.Protocol;

namespace Microsoft.Health.Fhir.Proxy.Pipelines
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
        }

        /// <summary>
        /// Creates an instance of OperationContext
        /// </summary>
        /// <param name="message">Initial http request message</param>
        public OperationContext(HttpRequestMessage message)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));

            Request = message;
            SetContentAsync(message).GetAwaiter();
            //long? contentLength = message.Content.Headers.ContentLength;
            //Stream stream = message.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            //byte[] buffer = new byte[(int)contentLength];
            //stream.Read(buffer, 0, buffer.Length);
            //Content = buffer;
            //Content = message.Content.Headers.ContentLength == null ? null : message.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            //Content = message.Content?.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        }

        private async Task SetContentAsync(HttpRequestMessage message)
        {
            long? contentLength = message.Content.Headers.ContentLength;
            if (!(contentLength == null || contentLength == 0))
            {
                Content = await message.Content.ReadAsByteArrayAsync();
            }
        }

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
        public void UpdateRequestUri(HttpMethod method, string routePrefix = "fhir", string resource = null, string id = null, string operation = null, string version = null)
        {
            FhirPath requestPath = new(method.ToString(), Request.RequestUri.ToString(), routePrefix);
            requestPath.Resource = resource ?? requestPath.Resource;
            requestPath.Id = id ?? requestPath.Id;
            requestPath.Operation = operation ?? requestPath.Operation;
            requestPath.Version = version ?? requestPath.Version;

            UriBuilder uriBuilder = new(this.Request.RequestUri);
            uriBuilder.Path = requestPath.Path;
            Request.RequestUri = uriBuilder.Uri;

            Request.Method = method;
        }
    }
}
