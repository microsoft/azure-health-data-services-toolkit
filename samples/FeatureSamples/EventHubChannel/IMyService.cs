using Microsoft.AzureHealth.DataServices.Channels;

namespace EventHubChannelSample
{
    public interface IMyService
    {
        event EventHandler<ChannelReceivedEventArgs> OnReceive;

        Task SendMessageAsync(HttpRequestMessage message);
    }
}
