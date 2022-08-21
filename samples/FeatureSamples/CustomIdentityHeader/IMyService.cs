using System.Collections.Specialized;

namespace CustomIdentityHeaderSample
{
    public interface IMyService
    {
        NameValueCollection GetHeaders(HttpRequestMessage message);
    }
}
