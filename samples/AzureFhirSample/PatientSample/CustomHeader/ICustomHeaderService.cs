using System.Collections.Specialized;
using System.Net.Http;

namespace CustomHeader
{
    public interface ICustomHeaderService
    {
        NameValueCollection GetHeaders(HttpRequestMessage message);
    }
}
