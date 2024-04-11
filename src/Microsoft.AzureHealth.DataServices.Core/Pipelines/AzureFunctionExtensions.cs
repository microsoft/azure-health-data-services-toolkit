using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AzureHealth.DataServices.Clients;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Microsoft.AzureHealth.DataServices.Pipelines
{
    /// <summary>
    /// Helper extensions for Azure Functions
    /// </summary>
    public static class AzureFunctionExtensions
    {
        /// <summary>
        /// Converts HttpRequestData to HttpRequestMessage.
        /// </summary>
        /// <param name="req">HttpRequestData</param>
        /// <returns>HttpRequestMessage</returns>
        public static HttpRequestMessage ConvertToHttpRequestMesssage(this HttpRequestData req)
        {
            HttpRequestMessage httpRequestMessage = new()
            {
                Method = new HttpMethod(req.Method),
            };

            HttpRequestMessage message = httpRequestMessage;
            message.RequestUri = req.Url;

            if (req.Body != null && req.Body.Length > 0)
            {
                message.Content = new StringContent(req.ReadAsString() ?? string.Empty);
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
            {
                if (HttpMessageExtensions.ContentHeaderNames.Any(x => string.Equals(x, header.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    message.Content ??= new ByteArrayContent(Array.Empty<byte>());
                    if (string.Equals(header.Key, Net.Http.Headers.HeaderNames.ContentType, StringComparison.OrdinalIgnoreCase))
                    {
                        // only include the first value for Content-Type
                        message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(header.Value.First());
                    }
                    else
                    {
                        message.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                else
                {
                    message.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return message;
        }

        /// <summary>
        /// Uses a combination of an HttpResponeMessage and HttpRequestData to conver to HttpResponseData.
        /// </summary>
        /// <param name="message">HttpRequestMessage that is extended.</param>
        /// <param name="request">HttpRequestData to use in the conversion.</param>
        /// <returns>HttpResponseData</returns>
        public static async Task<HttpResponseData> ConvertToHttpResponseDataAsync(this HttpResponseMessage message, HttpRequestData request)
        {
            HttpResponseData data = request.CreateResponse(message.StatusCode);

            NameValueCollection messageHeaders = message.GetHeaders();
            messageHeaders.Add(message.GetContentHeaders());

            foreach (var messageHeaderKey in messageHeaders.AllKeys)
            {
                if (messageHeaderKey is not null && messageHeaders[messageHeaderKey] is not null)
                {
                    data.Headers.TryAddWithoutValidation(messageHeaderKey, messageHeaders[messageHeaderKey]);
                }
            }

            string content = await message.Content.ReadAsStringAsync();

            if (content != null)
            {
                // Add Content-Type header if not already present.
                if (!data.Headers.Any(x => x.Key.ToLowerInvariant() == "content-type"))
                {
                    data.Headers.Add("Content-Type", "application/json");
                }

                var contentLength = message.Content.Headers.GetValues("Content-Length").FirstOrDefault();
                data.Headers.Add("Content-Length", contentLength);
                await data.WriteStringAsync(content);
            }

            return data;
        }

        /// <summary>
        /// Converts OperationContext to HttpResponseData.
        /// </summary>
        /// <param name="context">OperationContext</param>
        /// <param name="request">HttpRequestData</param>
        /// <returns>HttpResponseData</returns>
        public static HttpResponseData ConvertToHttpResponseData(this OperationContext context, HttpRequestData request)
        {
            HttpResponseData data = request.CreateResponse(context.StatusCode);

            if (context.IsFatal)
            {
                if ((int)data.StatusCode < 400)
                {
                    data.StatusCode = HttpStatusCode.InternalServerError;
                }
            }

            foreach (IHeaderNameValuePair header in context.Headers)
            {
                data.Headers.TryAddWithoutValidation(header.Name, header.Value);
            }

            data.StatusCode = context.StatusCode;

            return data;
        }

        /// <summary>
        /// Gets a ClaimsPrincipal from HttpRequestData.
        /// </summary>
        /// <param name="request">HttpRequestData</param>
        /// <returns>ClaimsPrincipal</returns>
        public static ClaimsPrincipal GetClaimsPrincipal(this HttpRequestData request)
        {
            if (!request.Headers.TryGetValues("Authorization", out IEnumerable<string> tokens))
            {
                return null;
            }

            string tokenString = tokens.ToList()[0];
            JsonWebToken jwt = new(tokenString);
            ClaimsIdentity identity = new(jwt.Claims);
            return new ClaimsPrincipal(identity);
        }

        /// <summary>
        /// Gets a claims principal from an http request with a bearer security token.
        /// </summary>
        /// <param name="request">Http request containing the security bearer token.</param>
        /// <returns>Claims principal object</returns>
        public static ClaimsPrincipal GetClaimsPrincipal(this HttpRequestMessage request)
        {
            string header = request.Headers.Authorization?.Parameter;
            if (header == null)
            {
                return null;
            }

            header = header.Replace("Bearer ", string.Empty);
            JsonWebToken jwt = new(header);
            ClaimsIdentity identity = new(jwt.Claims);
            return new ClaimsPrincipal(identity);
        }
    }
}
