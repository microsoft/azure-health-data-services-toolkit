using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
using Microsoft.AzureHealth.DataServices.Bindings;
using Microsoft.AzureHealth.DataServices.Pipelines;
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
        public GenericRestClient(Uri endpoint, TokenCredential tokenCredential, RestBindingOptions clientOptions, ILogger<GenericRestClient> logger = null)
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
        /// Default Content Type as Json
        /// </summary>
        private static string ContentType => "application/json";

        /// <summary>
        /// Sends and http request and returns a response.
        /// </summary>
        /// <returns>HttpResponseMessage</returns>
        public async Task<Response> SendAsync(OperationContext operationContext)
        {
            try
            {
                //string token = await FetchToken();
                using HttpMessage message = operationContext.Request.Method.ToString().ToUpperInvariant() switch
                {
                    "GET" => CreateRequest(operationContext),
                    "POST" => CreateRequest(operationContext),
                    "PUT" => CreateRequest(operationContext),
                    "DELETE" => CreateRequest(operationContext),
                    "PATCH" => CreateRequest(operationContext),
                    _ => throw new Exception("Invalid Http method."),
                };
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

        /// <summary>
        /// Create Http Message Request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private HttpMessage CreateRequest(OperationContext context)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = ConvertToRequestMethod(context.Request.Method.ToString());
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

        private static RequestMethod ConvertToRequestMethod(string method)
        {
            RequestMethod requestMethod = method.ToUpperInvariant() switch
            {
                "GET" => RequestMethod.Get,
                "POST" => RequestMethod.Post,
                "PUT" => RequestMethod.Put,
                "DELETE" => RequestMethod.Delete,
                "PATCH" => RequestMethod.Patch,
                _ => throw new Exception("Invalid Http method."),

            };
            return requestMethod;
        }

        private static string GetDefaultScope(Uri uri)
            => $"{uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped)}/.default";

        private static ResponseClassifier _responseClassifier200;
        private static ResponseClassifier ResponseClassifier200 => _responseClassifier200 ??= new StatusCodeClassifier(stackalloc ushort[] { 200 });

    }
}
