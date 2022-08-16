using DataServices.Clients.Headers;
using System.Collections.Specialized;

namespace CustomRequestHeadersSample
{
    public class MyService : IMyService
    {
        public MyService(IHttpCustomHeaderCollection customRequestHeaders)
        {
            this.customRequestHeaders = customRequestHeaders;
        }

        private readonly IHttpCustomHeaderCollection customRequestHeaders;

        public NameValueCollection GetHeaders(HttpRequestMessage message)
        {
            return customRequestHeaders.AppendAndReplace(message);
        }
    }
}
