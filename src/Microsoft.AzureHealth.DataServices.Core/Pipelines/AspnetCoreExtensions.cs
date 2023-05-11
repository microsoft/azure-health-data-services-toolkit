using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AzureHealth.DataServices.Pipelines
{
    /// <summary>
    /// Extensions to convert http request messages.
    /// </summary>
    public static class AspnetCoreExtensions
    {
        /// <summary>
        /// Converts HttpRequest to HttpRequestMessage.
        /// </summary>
        /// <param name="httpRequest">HttpRequest to convert.</param>
        /// <returns>HttpRequestMessage</returns>
        public static HttpRequestMessage ConvertToHttpRequestMessage(this HttpRequest httpRequest)
        {
            HttpRequestMessage httpRequestMessage = new()
            {
                // Set the HTTP method
                Method = new(httpRequest.Method),

                // Set the request URI
                RequestUri = new(httpRequest.Scheme + "://" + httpRequest.Host.Value + httpRequest.Path)
            };

            // Copy headers from HttpRequest to HttpRequestMessage
            foreach (var header in httpRequest.Headers)
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            // Copy the request body
            httpRequestMessage.Content = new StreamContent(httpRequest.Body);

            return httpRequestMessage;
        }
    }
}
