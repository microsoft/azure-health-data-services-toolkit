namespace EventGridChannelSample
{
    public interface IPipelineService
    {
        event EventHandler<ReceiveEventArgs> OnReceive;

        Task ExecuteAsync(HttpRequestMessage message);

    }
}
