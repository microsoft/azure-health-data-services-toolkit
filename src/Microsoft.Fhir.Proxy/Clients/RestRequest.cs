using Microsoft.Extensions.Logging;
using System;
using System.IO;
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
        public virtual async Task<HttpResponseMessage> SendAsync()
        {
            _ = builder ?? throw new Exception("Builder not set");

            try
            {
                logger?.LogTrace("Starting REST Send operation.");
                HttpWebRequest request = builder.Build();
                request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.None;
                logger?.LogTrace("REST request built.");

                if (builder.Content != null)
                {
                    using Stream stream = request.GetRequestStream();
                    await stream.WriteAsync(builder.Content.AsMemory(0, builder.Content.Length));
                    stream.Close();
                }

                using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                logger?.LogTrace($"Rest response returned status {response.StatusCode}.");
                logger?.LogTrace($"Rest response returned with content-type {response.ContentLength}.");

                if (response.ContentLength == 0)
                {
                    HttpResponseMessage resp = new(response.StatusCode);
                    if (!resp.IsSuccessStatusCode)
                    {
                        logger?.LogWarning($"Rest response returned fault reason phrase {response.StatusDescription}.");
                        resp.ReasonPhrase = response.StatusDescription;
                    }

                    logger?.LogInformation("Rest returning HTTP response.");
                    return resp;
                }

                byte[] buffer = new byte[blockSize];
                byte[] msg = null;

                logger?.LogTrace("REST response starting read.");
                using (Stream stream = response.GetResponseStream())
                {
                    using MemoryStream bufferStream = new();
                    int bytesRead;
                    do
                    {
                        bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
                        if (bytesRead > 0)
                        {
                            await bufferStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                        }
                    } while (bytesRead > 0);

                    logger?.LogTrace("REST response content read completed.");

                    if (bufferStream != null && bufferStream.Length > 0)
                    {
                        msg = bufferStream.ToArray();
                    }
                }

                HttpResponseMessage httpResponse = new(response.StatusCode);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    logger?.LogWarning($"Rest response returned fault reason phrase {response.StatusDescription}.");
                    httpResponse.ReasonPhrase = response.StatusDescription;
                }
                else
                {
                    httpResponse.Content = msg != null ? new ByteArrayContent(msg) : null;
                }

                logger?.LogInformation("Rest returning HTTP response.");
                return httpResponse;
            }
            catch (WebException wex)
            {
                logger?.LogError(wex, $"Rest web request faulted '{wex.Status}'.");
                throw;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Rest request faulted '{ex.Message}'.");
                throw;
            }
        }
    }
}
