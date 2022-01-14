using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using System.Net.Http;

namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    public static class AspnetCoreExtensions
    {
        /// <summary>
        /// Converts HttpRequest to HttpRequestMessage.
        /// </summary>
        /// <param name="req">HttpRequest to convert.</param>
        /// <returns>HttpRequestMessage</returns>
        public static HttpRequestMessage ConvertToHttpRequestMesssage(this HttpRequest req)
        {
            HttpRequestMessageFeature hreqmf = new(req.HttpContext);
            return hreqmf.HttpRequestMessage;
        }
    }
}
