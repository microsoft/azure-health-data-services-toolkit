using System.Collections.Specialized;
using System.Net.Http;

namespace QuickstartSample.CustomHeader
{
    public interface ICustomHeaderService
    {
        NameValueCollection GetHeaders(HttpRequestMessage message);
    }
}
