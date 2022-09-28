
using Microsoft.AzureHealth.DataServices.Channels;

namespace EventHubChannelSample
{
    public interface IMyService
    {
        Task SendMessageAsync(HttpRequestMessage message);

        event EventHandler<ChannelReceivedEventArgs> OnReceive;
    }
}
