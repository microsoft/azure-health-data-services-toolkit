using System.Collections.Specialized;
using Microsoft.AzureHealth.DataServices.Clients.Headers;

namespace CustomHeadersSample
{
    public class MyService : IMyService
    {
        private readonly IHttpCustomHeaderCollection customHeaders;

        public MyService(IHttpCustomHeaderCollection customHeaders)
        {
            this.customHeaders = customHeaders;
        }

        public NameValueCollection GetCustomHeaders(HttpRequestMessage message)
        {
            return customHeaders.RequestAppendAndReplace(message);
        }
    }
}
