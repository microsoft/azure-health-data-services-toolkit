using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;

namespace Azure.Health.DataServices.Pipelines
{
    /// <summary>
    /// Extensions to convert http request messages.
    /// </summary>
    public static class AspnetCoreExtensions
    {
        /// <summary>
        /// Converts HttpRequest to HttpRequestMessage.
        /// </summary>
        /// <param name="req">HttpRequest to convert.</param>
        /// <returns>HttpRequestMessage</returns>
        public static HttpRequestMessage ConvertToHttpRequestMessage(this HttpRequest req)
        {
            HttpRequestMessageFeature hreqmf = new(req.HttpContext);
            return hreqmf.HttpRequestMessage;
        }
    }
}
