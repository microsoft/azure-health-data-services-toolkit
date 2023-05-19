using System;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AzureHealth.DataServices.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AzureHealth.DataServices.Channels
{
    /// <summary>
    /// Channel that sends events to an Azure Event Grid.
    /// </summary>
    /// <remarks>The EventGridChannel and only send events, not receive.</remarks>
    public class EventGridChannel : IInputChannel, IOutputChannel
    {
        private readonly string _fallbackStorageConnectionString;
        private readonly string _container;
        private readonly string _topic;
        private readonly string _accessKey;
        private readonly string _subject;
        private readonly string _eventType;
        private readonly string _dataVersion;
        private readonly ILogger _logger;
        private readonly StatusType _statusType;
        private EventGridPublisherClient _client;
        private StorageBlob _storage;
        private bool _disposed;
        private ChannelState _state;

        /// <summary>
        /// Creates an instance of EventGridChannel.
        /// </summary>
        /// <param name="options">Options for sending to the event grid.</param>
        /// <param name="logger">ILogger</param>
        public EventGridChannel(IOptions<EventGridChannelOptions> options, ILogger<EventGridChannel> logger = null)
        {
            _fallbackStorageConnectionString = options.Value.FallbackStorageConnectionString;
            _container = options.Value.FallbackStorageContainer;
            _topic = options.Value.TopicUriString;
            _accessKey = options.Value.AccessKey;
            _subject = options.Value.Subject;
            _eventType = options.Value.EventType;
            _dataVersion = options.Value.DataVersion;
            _statusType = options.Value.ExecutionStatusType;
            _logger = logger;
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
        /// Gets the name of the channel, i.e., "EventGridChannel".
        /// </summary>
        public string Name => "EventGridChannel";

        /// <summary>
        /// Gets the requirement for executing the channel.
        /// </summary>
        public StatusType ExecutionStatusType => _statusType;

        /// <summary>
        /// Gets and indicator to whether the channel has authenticated the user, which is by default always false.
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
        /// Opens the channel.
        /// </summary>
        /// <returns>Task</returns>
        public async Task OpenAsync()
        {
            _client = new EventGridPublisherClient(
                        new Uri(_topic),
                        new AzureKeyCredential(_accessKey));

            _storage = new StorageBlob(_fallbackStorageConnectionString, null, null, null, _logger);

            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, Name, null));
            _logger?.LogInformation("{Name}-{Id} channel opened.", Name, Id);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Starts the receive operation for the channel.
        /// </summary>
        /// <returns>Task</returns>
        /// <remarks>Receive operation is omitted without error for an EventGridChannel.</remarks>
        public async Task ReceiveAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Sends a message to an Event Grid if size &lt; SKU constraint; otherwise uses blob storage.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="items">Additional optional parameters.</param>
        /// <returns>Task</returns>
        /// <remarks>Items argument is not used in EventGridChannel.</remarks>
        public async Task SendAsync(byte[] message, params object[] items)
        {
            try
            {
                EventGridEvent eventData = message.Length < Constants.EventGridMaxMessageLength ? new(_subject, _eventType, _dataVersion, message) : await GetBlobEventAsync(message);

                await _client.SendEventAsync(eventData);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                _logger?.LogError(ex, "{Name}-{Id} error attempting to send message.", Name, Id);
            }
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
        protected void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                _storage = null;
                _client = null;
                CloseAsync().GetAwaiter();
                _logger?.LogInformation("{Name}-{Id} channel disposed.", Name, Id);
            }
        }

        private async Task<EventGridEvent> GetBlobEventAsync(byte[] message)
        {
            // write the message to storage.
            string blobName = await WriteBlobAsync("text/plain", message);
            byte[] eventData = Encoding.UTF8.GetBytes($"{_container},{blobName}");

            // return the reference event;.
            return new EventGridEvent(_subject, "Reference", _dataVersion, eventData);
        }

        private async Task<string> WriteBlobAsync(string contentType, byte[] message)
        {
            string guid = Guid.NewGuid().ToString();
            string blob = $"{guid}T{DateTime.UtcNow:HH-MM-ss-fffff}";
            await _storage.WriteBlockBlobAsync(_container, blob, contentType, message);
            return blob;
        }
    }
}
