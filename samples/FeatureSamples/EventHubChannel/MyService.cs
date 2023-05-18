using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventHubChannelSample
{
    public class MyService : IMyService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IPipeline<HttpRequestMessage, HttpResponseMessage> _pipeline;
        private readonly IChannel receiveChannel;
        private bool _disposed;

        public MyService(MyServiceConfig config, IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline, ILogger<MyService> logger = null)
        {
            _pipeline = pipeline;
            _logger = logger;
            IOptions<EventHubChannelOptions> options = Options.Create<EventHubChannelOptions>(new EventHubChannelOptions()
            {
                ConnectionString = config.ConnectionString,
                ExecutionStatusType = config.ExecutionStatusType,
                FallbackStorageConnectionString = config.FallbackStorageConnectionString,
                FallbackStorageContainer = config.FallbackStorageContainer,
                HubName = config.HubName,
                ProcessorStorageContainer = config.ProcessorStorageContainer,
            });
            receiveChannel = new EventHubChannel(options);
        }

        public event EventHandler<ChannelReceivedEventArgs> OnReceive;

        public async Task SendMessageAsync(HttpRequestMessage message)
        {
            _logger?.LogInformation("Send message to event hub.");
            _ = _pipeline.ExecuteAsync(message);
            await OpenReceiveChannelAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                receiveChannel.Dispose();
            }
        }

        private async Task OpenReceiveChannelAsync()
        {
            _logger?.LogInformation("Open event hub receiver.");
            if (receiveChannel.State == ChannelState.None)
            {
                receiveChannel.OnReceive += ReceiveChannel_OnReceive;
                await receiveChannel.OpenAsync();
                receiveChannel.ReceiveAsync().GetAwaiter();
            }
        }

        private void ReceiveChannel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            _logger?.LogInformation("Event received from event hub.");
            OnReceive?.Invoke(sender, e);
        }
    }
}
