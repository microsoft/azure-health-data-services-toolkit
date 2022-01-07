using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Clients
{
    /// <summary>
    /// Makes an HTTP request to a web server.
    /// </summary>
    public class RestRequest
    {
        /// <summary>
        /// Creates an instance of the RestRequest.
        /// </summary>
        /// <param name="builder">REST request builder that creates the HttpWebRequest object.</param>
        /// <param name="blockSize">The number of bytes to be read in block; default is 16384.</param>
        /// <param name="logger">Optional logger.</param>
        public RestRequest(RestRequestBuilder builder, int blockSize = 16384, ILogger logger = null)
           : this(blockSize, logger)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            this.builder = builder;
        }

        protected RestRequest(int blockSize = 16384, ILogger logger = null)
        {
            this.blockSize = blockSize;
            this.logger = logger;
        }

        private readonly int blockSize;
        private readonly ILogger logger;
        private readonly RestRequestBuilder builder;

        /// <summary>
        /// Sends and http request and returns a response.
        /// </summary>
        /// <returns>HttpResponseMessage</returns>
        public async Task<HttpResponseMessage> SendAsync()
        {
            try
            {
                HttpClient client;
                HttpRequestMessage message = builder.Build();
                if (builder.Certificate != null)
                {
                    HttpClientHandler handler = new();
                    handler.ClientCertificates.Add(builder.Certificate);
                    client = new HttpClient(handler);
                }
                else
                {
                    client = new HttpClient
                    {
                        MaxResponseContentBufferSize = blockSize
                    };
                }

                HttpResponseMessage response = await client.SendAsync(message);
                logger?.LogInformation("Rest response returned status {0}.", response.StatusCode);
                logger?.LogTrace("Rest response returned with content-type {0}.", response.Content?.Headers.ContentType);

                if (response.IsSuccessStatusCode)
                {
                    logger?.LogInformation("Return http response.");
                }
                else
                {
                    logger?.LogWarning("Rest response returned fault reason phrase {0}.", response.ReasonPhrase);
                }

                return response;
            }
            catch (WebException wex)
            {
                logger?.LogError(wex, "Rest web request faulted '{0}'.", wex.Status);
                throw;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Rest request faulted '{0}'.", ex.Message);
                throw;
            }
        }
    }
}
