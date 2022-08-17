using Azure.Health.DataServices.Channels;

namespace ServiceBusChannelSample
{
    public interface IMyService
    {
        event EventHandler<ChannelReceivedEventArgs> OnReceive;

        Task SendAsync(HttpRequestMessage message);



    }
}
