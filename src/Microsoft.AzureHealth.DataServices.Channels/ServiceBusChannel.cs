using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AzureHealth.DataServices.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Microsoft.AzureHealth.DataServices.Channels
{
    /// <summary>
    /// Channel that can send or receive data from Azure Service Bus.
    /// </summary>
    public class ServiceBusChannel : IInputChannel, IOutputChannel
    {
        private readonly ILogger _logger;
        private readonly StatusType _statusType;
        private readonly string _connectionString;
        private readonly string _fallbackStorageConnectionString;
        private readonly string _storageContainer;
        private readonly string _topic;
        private readonly string _subscription;
        private readonly string _queue;
        private readonly ServiceBusSkuType _sku;
        private ChannelState _state;
        private StorageBlob _storage;
        private bool _disposed;
        private ServiceBusClient _client;
        private ServiceBusSender _sender;
        private ServiceBusProcessor _processor;

        /// <summary>
        /// Creates an instance of ServiceBusChannel for sending or receiving messages from service bus.
        /// </summary>
        /// <param name="options">Send options.</param>
        /// <param name="logger">ILogger</param>
        /// <remarks>The subscription option must be configured to receive events.</remarks>
        public ServiceBusChannel(IOptions<ServiceBusChannelOptions> options, ILogger<ServiceBusChannel> logger = null)
        {
            Id = Guid.NewGuid().ToString();
            _sku = options.Value.Sku;
            _connectionString = options.Value.ConnectionString;
            _fallbackStorageConnectionString = options.Value.FallbackStorageConnectionString;
            _storageContainer = options.Value.FallbackStorageContainer;
            _topic = options.Value.Topic;
            _subscription = options.Value.Subscription;
            _queue = options.Value.Queue;
            _statusType = options.Value.ExecutionStatusType;
            _logger = logger;
        }

        public ServiceBusChannel(ServiceBusSkuType sku)
        {
            _sku = sku;
        }

        /// <summary>
        /// Event that signals the channel has closed.
        /// </summary>
        public event EventHandler<ChannelCloseEventArgs> OnClose;

        /// <summary>
        /// Event that signals the channel has errored.
        /// </summary>
        public event EventHandler<ChannelErrorEventArgs> OnError;

        /// <summary>
        /// Event that signals the channel has opened.
        /// </summary>
        public event EventHandler<ChannelOpenEventArgs> OnOpen;

        /// <summary>
        /// Event that signals the channel as received a message.
        /// </summary>
        public event EventHandler<ChannelReceivedEventArgs> OnReceive;

        /// <summary>
        /// Event that signals the channel state has changed.
        /// </summary>
        public event EventHandler<ChannelStateEventArgs> OnStateChange;

        /// <summary>
        /// Gets the instance ID of the channel.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name of the channel, i.e., "ServiceBusChannel".
        /// </summary>
        public string Name => "ServiceBusChannel";

        /// <summary>
        /// Gets the requirement for executing the channel.
        /// </summary>
        public StatusType ExecutionStatusType => _statusType;

        /// <summary>
        /// Gets and indicator to whether the channel has authenticated the user, which by default always false.
        /// </summary>
        public bool IsAuthenticated => false;

        /// <summary>
        /// Indicates whether the channel is encrypted, which is always true.
        /// </summary>
        public bool IsEncrypted => true;

        /// <summary>
        /// Gets the port used, which by default always 0.
        /// </summary>
        public int Port => 0;

        /// <summary>
        /// Gets or sets the channel state.
        /// </summary>
        public ChannelState State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnStateChange?.Invoke(this, new ChannelStateEventArgs(Id, _state));
                }
            }
        }

        /// <summary>
        /// Add a message to the channel which is surface by the OnReceive event.
        /// </summary>
        /// <param name="message">Message to add.</param>
        /// <returns>Task</returns>
        public async Task AddMessageAsync(byte[] message)
        {
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, message));
            await Task.CompletedTask;
        }

        /// <summary>
        /// Opens the channel.
        /// </summary>
        /// <returns>Task</returns>
        public async Task OpenAsync()
        {
            if (string.IsNullOrEmpty(_fallbackStorageConnectionString))
            {
                _logger?.LogWarning("Service Bus channel not configured fallback storage connection string.");
            }
            else
            {
                _storage = new StorageBlob(_fallbackStorageConnectionString, null, 10, null, _logger);
            }

            _client = new(_connectionString);
            string resource = _topic ?? _queue;
            _sender = _client.CreateSender(resource);

            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, Name, null));
            _logger?.LogInformation("{Name}-{Id} opened.", Name, Id);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Sends a message to a Service Bus topic if size &lt; SKU constraint; otherwise uses blob storage.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="items">Additional optional parameters, where required is the content type.</param>
        public async Task SendAsync(byte[] message, params object[] items)
        {
            try
            {
                string contentType = (string)items[0];
                ServiceBusMessage data = null;

                if ((_sku != ServiceBusSkuType.Premium &&
                    message.Length > Constants.ServiceBusNonPremiumSkuMaxMessageLength) ||
                    (_sku == ServiceBusSkuType.Premium && message.Length > Constants.ServiceBusPremiumSkuMaxMessageLength))
                {
                    string blob = await WriteBlobAsync(contentType, message);
                    data = await GetBlobEventDataAsync(contentType, blob);
                }
                else
                {
                    data = await GetServiceBusEventDataAsync(contentType, message);
                }

                await _sender.SendMessageAsync(data);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                _logger?.LogError(ex, "{Name}-{Id} error attempting to send message.", Name, Id);
            }
        }

        /// <summary>
        /// Starts the receive operation for the channel.
        /// </summary>
        /// <returns>Task</returns>
        /// <remarks>Receive operation and subscription in Service Bus.</remarks>
        public async Task ReceiveAsync()
        {
            if (string.IsNullOrEmpty(_subscription) && string.IsNullOrEmpty(_queue))
            {
                throw new InvalidOperationException("Neither queue or subscription configured to receive");
            }

            if (!string.IsNullOrEmpty(_subscription) && !string.IsNullOrEmpty(_queue))
            {
                throw new InvalidOperationException("Both queue and subscription cannot be configured to receive");
            }

            try
            {
                ServiceBusProcessorOptions options = new()
                {
                    AutoCompleteMessages = true,
                    ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                };

                if (_queue != null)
                {
                    _processor = _client.CreateProcessor(_queue, options);
                    _logger?.LogInformation("{Name}-{Id} queue {queue} receiving.", Name, Id, _queue);
                }

                if (_subscription != null)
                {
                    _processor = _client.CreateProcessor(_topic, _subscription, options);
                    _logger?.LogInformation("{Name}-{Id} topic {topic} subscription {sub} receiving.", Name, Id, _topic, _subscription);
                }

                _processor.ProcessErrorAsync += async (args) =>
                {
                    _logger?.LogInformation("{Name}-{Id} received error in processor.", Name, Id);
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, args.Exception));
                    await Task.CompletedTask;
                };

                _processor.ProcessMessageAsync += async (args) =>
                {
                    ServiceBusReceivedMessage msg = args.Message;
                    _logger?.LogInformation("{Name}-{Id} message received.", Name, Id);

                    if (msg.ApplicationProperties.ContainsKey("PassedBy") && (string)msg.ApplicationProperties["PassedBy"] == "Reference")
                    {
                        EventDataByReference byRef = JsonConvert.DeserializeObject<EventDataByReference>(Encoding.UTF8.GetString(msg.Body.ToArray()));
                        var result = await _storage.ReadBlockBlobAsync(byRef.Container, byRef.Blob);
                        _logger?.LogInformation("{Name}-{Id} processing large message from blob.", Name, Id);
                        OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, result));
                    }
                    else
                    {
                        _logger?.LogInformation("{Name}-{Id} processing subscription message.", Name, Id);
                        OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, msg.Body.ToArray()));
                    }
                };

                await _processor.StartProcessingAsync();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                _logger?.LogError(ex, "{Name}-{Id} error receiving messages.", Name, Id);
            }
        }

        /// <summary>
        /// Closes the channel.
        /// </summary>
        /// <returns>Task</returns>
        public async Task CloseAsync()
        {
            if (State != ChannelState.Closed)
            {
                State = ChannelState.Closed;
                OnClose?.Invoke(this, new ChannelCloseEventArgs(Id, Name));
                _logger?.LogInformation("{Name}-{Id} channel closed.", Name, Id);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Disposes the channel.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">Indicator is true if disposing.</param>
        protected async void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;

                try
                {
                    _storage = null;

                    if (_sender != null)
                    {
                        await _sender.DisposeAsync();
                    }

                    if (_processor != null)
                    {
                        await _processor.StopProcessingAsync();
                        await _processor.DisposeAsync();
                    }

                    if (_client != null)
                    {
                        await _client.DisposeAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "{Name}-{Id} fault disposing.", Name, Id);
                }

                CloseAsync().GetAwaiter();
                _logger?.LogInformation("{Name}-{Id} disposed.", Name, Id);
            }
        }

        private static async Task<ServiceBusMessage> GetServiceBusEventDataAsync(string contentType, byte[] message)
        {
            ServiceBusMessage data = new(message);
            data.ContentType = contentType;
            return await Task.FromResult<ServiceBusMessage>(data);
        }

        private async Task<string> WriteBlobAsync(string contentType, byte[] message)
        {
            if (_storage == null)
            {
                var exception = new ServiceBusChannelException("Requires blob storage configured to write.");
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, exception));
                throw exception;
            }

            string guid = Guid.NewGuid().ToString();
            string blob = $"{guid}T{DateTime.UtcNow:HH-MM-ss-fffff}";
            await _storage.WriteBlockBlobAsync(_storageContainer, blob, contentType, message);
            return blob;
        }

        private async Task<ServiceBusMessage> GetBlobEventDataAsync(string contentType, string blobName)
        {
            if (_storageContainer == null)
            {
                var exception = new ServiceBusChannelException("Requires blob storage container configured to write event data.");
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, exception));
                throw exception;
            }

            EventDataByReference byref = new(_storageContainer, blobName, contentType);
            string json = JsonConvert.SerializeObject(byref);
            ServiceBusMessage data = new(Encoding.UTF8.GetBytes(json));
            data.ApplicationProperties.Add("PassedBy", "Reference");
            data.ContentType = contentType;
            return await Task.FromResult<ServiceBusMessage>(data);
        }
    }
}
