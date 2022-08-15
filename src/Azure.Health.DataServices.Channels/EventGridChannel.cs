using System;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventGrid;
using Azure.Health.DataServices.Channels;
using Azure.Health.DataServices.Pipelines;
using Azure.Health.DataServices.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azure.Health.DataServices.Channels
{
    /// <summary>
    /// Channel that sends events to an Azure Event Grid.
    /// </summary>
    /// <remarks>The EventGridChannel and only send events, not receive.</remarks>
    public class EventGridChannel : IInputChannel, IOutputChannel
    {
        /// <summary>
        /// Creates an instance of EventGridChannel.
        /// </summary>
        /// <param name="options">Options for sending to the event grid.</param>
        /// <param name="logger">ILogger</param>
        public EventGridChannel(IOptions<EventGridChannelOptions> options, ILogger<EventGridChannel> logger = null)
        {
            fallbackStorageConnectionString = options.Value.FallbackStorageConnectionString;
            container = options.Value.FallbackStorageContainer;
            topic = options.Value.TopicUriString;
            accessKey = options.Value.AccessKey;
            subject = options.Value.Subject;
            eventType = options.Value.EventType;
            dataVersion = options.Value.DataVersion;
            statusType = options.Value.ExecutionStatusType;
            this.logger = logger;
        }

        private readonly string fallbackStorageConnectionString;
        private readonly string container;
        private readonly string topic;
        private readonly string accessKey;
        private readonly string subject;
        private readonly string eventType;
        private readonly string dataVersion;
        private EventGridPublisherClient client;
        private StorageBlob storage;
        private bool disposed;
        private ChannelState state;
        private readonly ILogger logger;
        private readonly StatusType statusType;

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
        public StatusType ExecutionStatusType => statusType;

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
            get => state;
            set
            {
                if (state != value)
                {
                    state = value;
                    OnStateChange?.Invoke(this, new ChannelStateEventArgs(Id, state));
                }
            }
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
                logger?.LogInformation("{Name}-{Id} channel closed.", Name, Id);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Opens the channel.
        /// </summary>
        /// <returns>Task</returns>
        public async Task OpenAsync()
        {
            client = new EventGridPublisherClient(
                        new Uri(topic),
                        new AzureKeyCredential(accessKey));

            storage = new StorageBlob(fallbackStorageConnectionString, null, null, null, logger);

            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, Name, null));
            logger?.LogInformation("{Name}-{Id} channel opened.", Name, Id);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Starts the recieve operation for the channel.
        /// </summary>
        /// <returns>Task</returns>
        /// <remarks>Receive operation is omitted without error for an EventGridChannel.</remarks>

        public async Task ReceiveAsync()
        {
            await Task.CompletedTask;
        }



        /// <summary>
        ///Sends a message to an Event Grid if size &lt; SKU constraint; otherwise uses blob storage.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="items">Additional optional parameters.</param>
        /// <returns>Task</returns>
        /// <remarks>Items argument is not used in EventGridChannel.</remarks>
        public async Task SendAsync(byte[] message, params object[] items)
        {
            try
            {
                EventGridEvent eventData = message.Length < Constants.EventGridMaxMessageLength ? new(subject, eventType,
                                               dataVersion, message) : await GetBlobEventAsync(message);

                await client.SendEventAsync(eventData);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                logger?.LogError(ex, "{Name}-{Id} error attempting to send message.", Name, Id);
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

        protected void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;
                storage = null;
                client = null;
                CloseAsync().GetAwaiter();
                logger?.LogInformation("{Name}-{Id} channel disposed.", Name, Id);
            }
        }


        private async Task<EventGridEvent> GetBlobEventAsync(byte[] message)
        {
            //write the message to storage.
            string blobName = await WriteBlobAsync("text/plain", message);
            byte[] eventData = Encoding.UTF8.GetBytes($"{container},{blobName}");
            //return the reference event;.
            return new EventGridEvent(subject, "Reference", dataVersion, eventData);
        }

        private async Task<string> WriteBlobAsync(string contentType, byte[] message)
        {
            string guid = Guid.NewGuid().ToString();
            string blob = $"{guid}T{DateTime.UtcNow:HH-MM-ss-fffff}";
            await storage.WriteBlockBlobAsync(container, blob, contentType, message);
            return blob;
        }
    }
}
