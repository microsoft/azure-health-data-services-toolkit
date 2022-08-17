using Azure.Health.DataServices.Clients.Headers;
using System.Collections.Specialized;

namespace CustomIdentityHeaderSample
{
    public class MyService : IMyService
    {
        public MyService(IHttpCustomHeaderCollection customIdentityHeaders)
        {
            this.customIdentityHeaders = customIdentityHeaders;
        }

        private readonly IHttpCustomHeaderCollection customIdentityHeaders;

        public NameValueCollection GetHeaders(HttpRequestMessage message)
        {
            return customIdentityHeaders.AppendAndReplace(message);
        }
    }
}
