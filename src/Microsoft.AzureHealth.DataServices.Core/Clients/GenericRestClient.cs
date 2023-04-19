using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AzureHealth.DataServices.Clients
{
    /// <summary>
    /// Generic REST client for interacting with Azure services.
    /// </summary>
    public class GenericRestClient
    {
        /// <summary>
        /// Protected constructor to allow mocking.
        /// </summary>
        protected GenericRestClient()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRestClient"/> class.
        /// </summary>
        /// <param name="endpoint">#TODO</param>
        ///  <param name="credential">#TODO</param>
        /// <param name="options">#TODO</param>
        /// <param name="logger">#TODO</param>

        public GenericRestClient(Uri endpoint, GenericRestClientOptions options, TokenCredential credential = null, ILogger<GenericRestClient> logger = null) : this(endpoint, (ClientOptions)options, credential, logger)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRestClient"/> class.
        /// </summary>
        /// <param name="endpoint">#TODO</param>
        ///  <param name="credential">#TODO</param>
        /// <param name="options">#TODO</param>
        /// <param name="logger">#TODO</param>

        public GenericRestClient(Uri endpoint, ClientOptions options, TokenCredential credential = null, ILogger<GenericRestClient> logger = null)
        {
            if (endpoint is null)
                throw new ArgumentNullException(nameof(endpoint));
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            _logger = logger;

            _pipeline = CreatePipeline(endpoint, options, credential);
        }

        private readonly ILogger _logger;
        private readonly HttpPipeline _pipeline;

        /// <summary> The HTTP pipeline for sending and receiving REST requests and responses. </summary>
        public virtual HttpPipeline Pipeline => _pipeline;

        /// <summary> Default ContentType for requests to JSON. </summary>
        private static string ContentType => "application/json";

        private static HttpPipeline CreatePipeline(Uri endpoint, ClientOptions options, TokenCredential credential = null)
        {
            if (credential is null)
            {
                return HttpPipelineBuilder.Build(options,
                    Array.Empty<HttpPipelinePolicy>(),
                    new HttpPipelinePolicy[] { },
                    new ResponseClassifier()
                );
            }

            return HttpPipelineBuilder.Build(options, 
                Array.Empty<HttpPipelinePolicy>(), 
                new HttpPipelinePolicy[] { 
                    new BearerTokenAuthenticationPolicy(credential, GetDefaultScope(endpoint))
                },
                new ResponseClassifier()
            );
        }

        /// <summary>
        /// Sends and http request and returns a response.
        /// </summary>
        /// <returns>HttpResponseMessage</returns>

        internal virtual async Task<Response> SendAsync(HttpMessage message, AuthenticationHeaderValue? authOverride = null, CancellationToken cancellationToken = default)
        {           
            try
            {
                message = SanitizeRequest(message);

                if (authOverride is not null)
                {
                    message.Request.Headers.SetValue("Authorization", $"{authOverride.Scheme} {authOverride.Parameter}");
                }

                await _pipeline.SendAsync(message, cancellationToken);
                var response = message.Response;

                _logger?.LogTrace("GenericRestClient response returned status {StatusCode}.", response);

                if (response.Status == (int)HttpStatusCode.OK)
                {
                    _logger?.LogInformation("Return http response.");
                }
                else
                {
                    _logger?.LogWarning("Rest response returned fault reason phrase {ReasonPhrase}.", response.ReasonPhrase);
                }

                return response;
            }
            catch (WebException wex)
            {
                _logger?.LogError(wex, "Rest web request faulted '{Status}'.", wex.Status);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Rest request faulted '{Message}'.", ex.Message);
                throw;
            }
        }

        private HttpMessage SanitizeRequest (HttpMessage message)
        {
            message.Request.Headers.Remove("Content-Type");
            message.Request.Headers.Remove("Content-Length");
            message.Request.Headers.Remove("Authorization");
            message.Request.Headers.Remove("Accept");
            message.Request.Headers.Remove("Host");
            message.Request.Headers.Remove("User-Agent");
            return message;
        }

        private static string GetDefaultScope(Uri uri)
            => $"{uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped)}/.default";
    }
}
