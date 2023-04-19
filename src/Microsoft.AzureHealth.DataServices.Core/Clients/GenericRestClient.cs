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
        ///  <param name="tokenCredential">#TODO</param>
        /// <param name="clientOptions">#TODO</param>
        /// <param name="logger">#TODO</param>

        // #TODO - sorry I was wrong, the options class should be GenericRestClientOptions
        // RestBindingOptions should not be used for client optioins because GenericRestClient should be lower level than RestBinding.
        // Empty GenericRestClientOptions class is better
        public GenericRestClient(Uri endpoint, GenericRestClientOptions clientOptions, TokenCredential tokenCredential, ILogger<GenericRestClient> logger = null)
        {
            if (endpoint is null) 
                throw new ArgumentNullException(nameof(endpoint));
            if (clientOptions is null)
                throw new ArgumentNullException(nameof(endpoint));

            // #TODO - "token passthrough" would be enabled by this parameter being null and the token being passed in a below method.
            if (tokenCredential is null)
                throw new ArgumentNullException(nameof(tokenCredential));

            _endpoint = endpoint;
            _tokenCredential = tokenCredential;
            _logger = logger;

            _pipeline = CreatePipeline(endpoint, clientOptions, tokenCredential);
        }

        private readonly ILogger _logger;
        private readonly TokenCredential _credential;
        private readonly HttpPipeline _pipeline;
        private readonly Uri _endpoint;

        /// <summary> The HTTP pipeline for sending and receiving REST requests and responses. </summary>
        public virtual HttpPipeline Pipeline => _pipeline;
        
        /// <summary>
        /// Default ContentType for requests to JSON.
        /// </summary>
        private static string ContentType => "application/json";

        private static HttpPipeline CreatePipeline(Uri endpoint, GenericRestClientOptions options, TokenCredential credential)
        {
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

        // #TODO - this class should not take an OperationContext because then this client is tightly bound to the Toolkit pipeline and it limits the ability to use this client in other scenarios.
        // Instead it should take a HttpRequestMessage or other lower level types.
        // I would suggest making this class take a HttpMessage and move the logic to convert from OperationContext to HttpMessage into the Pipelines folder. For example you can see AzureFunctionExtensions.
        // Or have two methods
        public async Task<Response> SendAsync(OperationContext operationContext)
        {
            try
            {
                HttpMessage message = CreateRequest(operationContext);
                await _pipeline.SendAsync(message, CancellationToken.None);
                var response = message.Response;

                // #TODO - not sure the logging is useful here. pattern from the Azure SDK is to throw exceptions and use the ClientDiagnostics class
                // https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/appconfiguration/Azure.Data.AppConfiguration/src/ConfigurationClient.cs#L201


                _logger?.LogInformation("GenericRestClient response returned status {StatusCode}.", response);
                _logger?.LogTrace("GenericRestClient response returned with content-type {ContentType}.", response?.Headers.ContentType);

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
