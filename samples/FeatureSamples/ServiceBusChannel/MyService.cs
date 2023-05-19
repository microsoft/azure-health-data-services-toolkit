using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ServiceBusChannelSample
{
    public class MyService : IMyService, IDisposable
    {
        private readonly IChannel _channel;
        private readonly IPipeline<HttpRequestMessage, HttpResponseMessage> _pipeline;
        private readonly ILogger _logger;
        private bool _disposed;

        public MyService(MyServiceConfig config, IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline, ILogger<MyService> logger = null)
        {
            _pipeline = pipeline;
            _logger = logger;

            IOptions<ServiceBusChannelOptions> options = Options.Create<ServiceBusChannelOptions>(new ServiceBusChannelOptions()
            {
                ConnectionString = config.ConnectionString,
                ExecutionStatusType = config.ExecutionStatusType,
                FallbackStorageConnectionString = config.FallbackStorageConnectionString,
                FallbackStorageContainer = config.FallbackStorageContainer,
                Sku = config.Sku,
                Topic = config.Topic,
                Subscription = config.Subscription,
            });
            _channel = new ServiceBusChannel(options);
            _channel.OnReceive += Channel_OnReceive;
        }

        public event EventHandler<ChannelReceivedEventArgs> OnReceive;

        public async Task SendAsync(HttpRequestMessage message)
        {
            _logger?.LogInformation("Sending message to service bus.");
            await _pipeline.ExecuteAsync(message);
            await ReceiveAsync();
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
                _channel.Dispose();
            }
        }

        private async Task ReceiveAsync()
        {
            _logger?.LogInformation("Open service bus receiver.");
            if (_channel.State == ChannelState.None)
            {
                await _channel.OpenAsync();
                _channel.ReceiveAsync().GetAwaiter();
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            _logger?.LogInformation("Event received from serivce bus.");
            OnReceive?.Invoke(this, e);
        }
    }
}
