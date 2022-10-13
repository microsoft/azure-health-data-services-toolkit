using Microsoft.AzureHealth.DataServices.Clients.Headers;
using System.Collections.Specialized;

namespace CustomHeadersSample
{
    public class MyService : IMyService
    {
        public MyService(IHttpCustomHeaderCollection customHeaders)
        {
            this.customHeaders = customHeaders;
        }

        private readonly IHttpCustomHeaderCollection customHeaders;

        public NameValueCollection GetCustomHeaders(HttpRequestMessage message)
        {
            return customHeaders.RequestAppendAndReplace(message);
        }
    }
}
