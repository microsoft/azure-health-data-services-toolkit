using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Pipelines
{
    public static class Extensions
    {
        public static HttpRequestMessage ConvertToHttpRequestMesssage(this HttpRequestData req)
        {
            HttpRequestMessage message = new()
            {
                Method = new HttpMethod(req.Method)
            };

            message.RequestUri = req.Url;

            if (req.Body != null && req.Body.Length > 0)
            {
                message.Content = new StringContent(req.ReadAsString());
            }

            foreach (var header in req.Headers)
            {
                if (header.Key.ToLowerInvariant() == "content-type")
                {
                    message.Content.Headers.ContentType = new MediaTypeHeaderValue(header.Value.ToArray()[0]);
                }
                else if (header.Key.ToLowerInvariant() == "content-length")
                {
                    message.Content.Headers.ContentLength = Convert.ToInt64(header.Value.ToArray()[0]);
                }
                else
                {
                    message.Headers.Add(header.Key, header.Value);
                }
            }

            return message;
        }

        public static async Task<HttpResponseData> ConvertToHttpResponseData(this OperationContext context, HttpRequestData request)
        {
            HttpResponseData data = request.CreateResponse(context.StatusCode);

            if (context.IsFatal)
            {
                if ((int)data.StatusCode < 400)
                {
                    data.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                }
                else
                {
                    data.StatusCode = context.StatusCode;
                }

                return data;
            }

            if (!string.IsNullOrEmpty(context.ContentString))
            {
                data.Headers.Add("Content-Type", "application/json");
                data.Headers.Add("Content-Length", context.ContentString.Length.ToString());
                await data.WriteStringAsync(context.ContentString);
            }

            data.StatusCode = context.StatusCode;

            return data;
        }

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
    }
}
