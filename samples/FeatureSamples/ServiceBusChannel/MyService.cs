using Azure.Health.DataServices.Channels;
using Azure.Health.DataServices.Pipelines;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusChannelSample
{
    public class MyService : IMyService
    {
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

        private readonly IChannel _channel;
        private readonly IPipeline<HttpRequestMessage, HttpResponseMessage> _pipeline;
        private readonly ILogger _logger;

        public event EventHandler<ChannelReceivedEventArgs> OnReceive;

        public async Task SendAsync(HttpRequestMessage message)
        {
            _logger?.LogInformation("Sending message to service bus.");
            await _pipeline.ExecuteAsync(message);
            await ReceiveAsync();
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
