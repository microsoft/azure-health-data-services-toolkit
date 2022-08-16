using System.Collections.Specialized;

namespace CustomRequestHeadersSample
{
    public interface IMyService
    {
        NameValueCollection GetHeaders(HttpRequestMessage message);
    }
}
