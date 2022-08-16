using System.Collections.Specialized;

namespace CustomHeadersSample
{
    public interface IMyService
    {
        NameValueCollection GetCustomHeaders(HttpRequestMessage message);
    }
}
