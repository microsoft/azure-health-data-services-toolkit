namespace BlobChannelSample
{
    public interface IPipelineService
    {
        Task ExecuteAsync(HttpRequestMessage message);
    }
}
