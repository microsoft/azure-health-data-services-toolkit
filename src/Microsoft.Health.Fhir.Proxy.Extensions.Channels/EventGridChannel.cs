using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Extensions.Channels.Configuration;
using Microsoft.Health.Fhir.Proxy.Storage;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels
{
    public class EventGridChannel : IChannel
    {
        public EventGridChannel(EventGridConfig config, ILogger logger = null)
        {
            this.config = config;
            this.logger = logger;
        }

        private readonly EventGridConfig config;
        private EventGridPublisherClient client;
        private StorageBlob storage;
        private bool disposed;
        private ChannelState state;
        private readonly ILogger logger;

        public string Id { get; private set; }

        public string Name => "EventGridChannel";

        public bool IsAuthenticated => false;

        public bool IsEncrypted => true;

        public int Port => 0;

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

        public event EventHandler<ChannelCloseEventArgs> OnClose;
        public event EventHandler<ChannelErrorEventArgs> OnError;
        public event EventHandler<ChannelOpenEventArgs> OnOpen;
        public event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public event EventHandler<ChannelStateEventArgs> OnStateChange;

        public async Task AddMessageAsync(byte[] message)
        {
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, message));
            await Task.CompletedTask;
        }

        public async Task CloseAsync()
        {
            if (State != ChannelState.Closed)
            {
                State = ChannelState.Closed;
                OnClose?.Invoke(this, new ChannelCloseEventArgs(Id, Name));
                logger?.LogInformation($"{Name}-{Id} channel closed.");
            }

            await Task.CompletedTask;
        }

        public async Task OpenAsync()
        {
            client = new EventGridPublisherClient(
                        new Uri(config.EventGridTopicUriString),
                        new AzureKeyCredential(config.EventGridTopicAccessKey));

            storage = new StorageBlob(config.EventGridBlobConnectionString, null, null, null, logger);

            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, Name, null));
            logger?.LogInformation($"{Name}-{Id} channel opened.");

            await Task.CompletedTask;
        }

        public async Task ReceiveAsync()
        {
            await Task.CompletedTask;
        }

        public async Task SendAsync(byte[] message, params object[] items)
        {
            try
            {
                EventGridEvent eventData = message.Length < 1000000 ? new(config.EventGridSubject, config.EventGridEventType,
                                               config.EventGridDataVersion, message) : await GetBlobEventAsync(message);

                await client.SendEventAsync(eventData);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                logger?.LogError(ex, $"{Name}-{Id} error attempting to send message.");
            }
        }

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
                logger?.LogInformation($"{Name}-{Id} channel disposed.");
            }
        }


        private async Task<EventGridEvent> GetBlobEventAsync(byte[] message)
        {
            //write the message to storage.
            string blobName = await WriteBlobAsync("text/plain", message);
            byte[] eventData = Encoding.UTF8.GetBytes($"{config.EventGridBlobContainer},{blobName}");
            //return the reference event;.
            return new EventGridEvent(config.EventGridSubject, "Reference", config.EventGridDataVersion, eventData);
        }

        private async Task<string> WriteBlobAsync(string contentType, byte[] message)
        {
            string guid = Guid.NewGuid().ToString();
            string blob = $"{guid}T{DateTime.UtcNow:HH-MM-ss-fffff}";
            await storage.WriteBlockBlobAsync(config.EventGridBlobContainer, blob, contentType, message);
            return blob;
        }
    }
}
