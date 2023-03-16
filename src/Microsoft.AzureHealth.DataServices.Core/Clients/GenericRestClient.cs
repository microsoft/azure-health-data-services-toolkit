using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Microsoft.AzureHealth.DataServices.Protocol;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureHealth.DataServices.Clients
{
    /// <summary>
    /// Make Http Request to a web server.
    /// </summary>
    public class GenericRestClient
    {
        /// <summary>
        /// 
        /// </summary>
        public GenericRestClient()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="clientOptions"></param>
        /// <param name="tokenCredential"></param>
        /// <param name="logger"></param>
        public GenericRestClient(Uri endpoint, TokenCredential tokenCredential, GenericRestClientOption clientOptions, ILogger<GenericRestClient> logger = null)
        {
            this._tokenCredential = tokenCredential;
            _endpoint = endpoint;
            _pipeline = HttpPipelineBuilder.Build(clientOptions, Array.Empty<HttpPipelinePolicy>(), new HttpPipelinePolicy[] { new BearerTokenAuthenticationPolicy(_tokenCredential, GetDefaultScope(endpoint)) }, new ResponseClassifier());
            this.logger = logger;
        }

        private readonly ILogger logger;
        private readonly TokenCredential _tokenCredential;
        private readonly HttpPipeline _pipeline;
        private readonly Uri _endpoint;

        /// <summary> The HTTP pipeline for sending and receiving REST requests and responses. </summary>
        public virtual HttpPipeline Pipeline => _pipeline;


        /// <summary>
        /// Sends and http request and returns a response.
        /// </summary>
        /// <returns>HttpResponseMessage</returns>
        public async Task<Response> SendAsync(string body, CancellationToken cancellationToken = default)
        {
            try
            {
                RequestContext context = new RequestContext()
                {
                    CancellationToken = cancellationToken,
                };
                using HttpMessage message = CreatePostSecretRequest(body, context);
                await _pipeline.SendAsync(message, CancellationToken.None);
                var response = message.Response;
                logger?.LogInformation("Rest response returned status {StatusCode}.", response);
                logger?.LogTrace("Rest response returned with content-type {ContentType}.", response?.Headers.ContentType);

                if (response.Status == (int)HttpStatusCode.OK)
                {
                    logger?.LogInformation("Return http response.");
                }
                else
                {
                    logger?.LogWarning("Rest response returned fault reason phrase {ReasonPhrase}.", response.ReasonPhrase);
                }

                return response;
            }
            catch (WebException wex)
            {
                logger?.LogError(wex, "Rest web request faulted '{Status}'.", wex.Status);
                throw;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Rest request faulted '{Message}'.", ex.Message);
                throw;
            }
        }

        internal HttpMessage CreatePostSecretRequest(string body, RequestContext context)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = RequestMethod.Post;
            request.Content = RequestContent.Create(Encoding.UTF8.GetBytes(body));
            request.Headers.Add("Content-Type", "application/json");
            request.Uri.Reset(_endpoint);
            return message;
        }


        private static string GetDefaultScope(Uri uri)
            => $"{uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped)}/.default";

        private static ResponseClassifier _responseClassifier200;
        private static ResponseClassifier ResponseClassifier200 => _responseClassifier200 ??= new StatusCodeClassifier(stackalloc ushort[] { 200 });

    }
}
