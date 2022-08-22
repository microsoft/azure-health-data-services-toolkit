using Azure.Health.DataServices.Clients.Headers;
using System.Collections.Specialized;
using System.Net.Http;

namespace CustomHeader
{
    public class CustomHeaderService : ICustomHeaderService
    {
        public CustomHeaderService(IHttpCustomHeaderCollection customHeaders)
        {
            this.customHeaders = customHeaders;
        }

        private readonly IHttpCustomHeaderCollection customHeaders;

        public NameValueCollection GetHeaders(HttpRequestMessage message)
        {
            return customHeaders.AppendAndReplace(message);
        }
    }
}
