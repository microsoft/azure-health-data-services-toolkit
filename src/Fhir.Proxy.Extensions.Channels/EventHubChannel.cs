using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Fhir.Proxy.Channels;
using Fhir.Proxy.Pipelines;
using Fhir.Proxy.Storage;
using Newtonsoft.Json;

namespace Fhir.Proxy.Extensions.Channels
{
    /// <summary>
    /// Channel that sends events to an Azure Event Hub.
    /// </summary>
    public class EventHubChannel : IInputChannel, IOutputChannel
    {
        /// <summary>
        /// Creates an instance of EventHubChannel for sending to an event hub.
        /// </summary>
        /// <param name="options">Send options.</param>
        /// <param name="logger">ILogger</param>
        public EventHubChannel(IOptions<EventHubSendOptions> options, ILogger<EventHubChannel> logger = null)
        {
            sku = options.Value.Sku;
            connectionString = options.Value.ConnectionString;
            hubName = options.Value.HubName;
            statusType = options.Value.ExecutionStatusType;
            fallbackStorageConnectionString = options.Value.FallbackStorageConnectionString;
            fallbackContainer = options.Value.FallbackStorageContainer;
            this.logger = logger;
        }

        /// <summary>
        /// Creates an instance of EventHubChannel for receiving from an event hub.
        /// </summary>
        /// <param name="options">Receive options.</param>
        /// <param name="logger">ILogger</param>
        public EventHubChannel(IOptions<EventHubReceiveOptions> options, ILogger<EventHubChannel> logger = null)
        {
            connectionString = options.Value.ConnectionString;
            hubName = options.Value.HubName;
            processorContainer = options.Value.ProcessorStorageContainer;
            fallbackStorageConnectionString = options.Value.StorageConnectionString;
            this.logger = logger;
        }

        private ChannelState state;
        private readonly EventHubSkuType sku;
        private readonly string connectionString;
        private readonly string hubName;
        private readonly string processorContainer;
        private readonly string fallbackStorageConnectionString;
        private readonly string fallbackContainer;
        private EventHubProducerClient sender;
        private EventProcessorClient processor;
        private readonly ILogger logger;
        private readonly StatusType statusType;
        private StorageBlob storage;
        private bool disposed;

        /// <summary>
        /// Gets the instance ID of the channel.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name of the channel, i.e., "EventHubChannel".
        /// </summary>
        public string Name => "EventHubChannel";

        /// <summary>
        /// Gets the requirement for executing the channel.
        /// </summary>
        public StatusType ExecutionStatusType => statusType;

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
        /// Opens the channel.
        /// </summary>
        /// <returns>Task</returns>
        public async Task OpenAsync()
        {
            sender = new EventHubProducerClient(connectionString, hubName);
            storage = new StorageBlob(fallbackStorageConnectionString);

            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, Name, null));
            logger?.LogInformation("{Name}-{Id} channel opened.", Name, Id);
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
        /// Starts the recieve operation for the channel.
        /// </summary>
        /// <returns>Task</returns>
        /// <remarks>Receive operation uses the EventHubProcessor.</remarks>
        public async Task ReceiveAsync()
        {
            try
            {
                string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
                var storageClient = new BlobContainerClient(fallbackStorageConnectionString, processorContainer);
                processor = new EventProcessorClient(storageClient, consumerGroup, connectionString, hubName);

                processor.ProcessEventAsync += async (args) =>
                {
                    await args.UpdateCheckpointAsync(args.CancellationToken);
                    if (args.Data.Properties.ContainsKey("PassedBy") && (string)args.Data.Properties["PassedBy"] == "Value")
                    {
                        OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, args.Data.EventBody.ToArray()));
                    }

                    if (args.Data.Properties.ContainsKey("PassedBy") && (string)args.Data.Properties["PassedBy"] == "Reference")
                    {
                        var byRef = JsonConvert.DeserializeObject<EventDataByReference>(Encoding.UTF8.GetString(args.Data.EventBody.ToArray()));
                        var result = await storage.DownloadBlockBlobAsync(byRef.Container, byRef.Blob);
                        OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, result.Content.ToArray()));
                    }
                };

                processor.ProcessErrorAsync += async (args) =>
                {
                    //log exception
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, args.Exception));
                    logger?.LogError(args.Exception, "{Name}-{Id} event hub channel processor receive error.", Name, Id);
                    await Task.CompletedTask;
                };

                await processor.StartProcessingAsync();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                logger?.LogError(ex, "{Name}-{Id} event hub channel error receiving messages.", Name, Id);
            }
        }

        /// <summary>
        /// Sends a message to an Event Hub if size &lt; SKU constraint; otherwise uses blob storage.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="items">Additional optional parameters, where required is the content type.</param>
        /// <returns>Task</returns>
        public async Task SendAsync(byte[] message, params object[] items)
        {
            try
            {
                string typeName = "Value";

                if ((sku == EventHubSkuType.Basic && message.Length > 0x3E800) || (sku != EventHubSkuType.Basic && message.Length > 0xF4240))
                {
                    typeName = "Reference";
                }

                EventData data = null;
                string contentType = (string)items[0];

                if (typeName == "Reference")
                {
                    string blob = await WriteBlobAsync(contentType, message);
                    data = await GetBlobEventDataAsync(contentType, blob, typeName);
                }
                else
                {
                    data = await GetEventHubEventDataAsync(contentType, typeName, message);
                }

                using EventDataBatch eventBatch = await sender.CreateBatchAsync();
                if (eventBatch.TryAdd(data))
                {
                    await sender.SendAsync(eventBatch);
                    logger?.LogInformation("{Name}-{Id} event hub channel message sent.", Name, Id);
                }
                else
                {
                    Exception ex = new("Event hub could not send message.");
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                    logger?.LogError(ex, "{Name}-{Id} event hub error sending message.", Name, Id);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                logger?.LogError(ex, "{Name}-{Id} event hub error attempting to send message.", Name, Id);
            }
        }

        private async Task<string> WriteBlobAsync(string contentType, byte[] message)
        {
            string guid = Guid.NewGuid().ToString();
            string blob = $"{guid}T{DateTime.UtcNow:HH-MM-ss-fffff}";
            await storage.WriteBlockBlobAsync(fallbackContainer, blob, contentType, message);
            return blob;
        }

        private async Task<EventData> GetBlobEventDataAsync(string contentType, string blobName, string typeName)
        {
            EventDataByReference byref = new(fallbackContainer, blobName, contentType);
            string json = JsonConvert.SerializeObject(byref);
            EventData data = new(Encoding.UTF8.GetBytes(json));
            data.Properties.Add("PassedBy", typeName);
            data.ContentType = contentType;
            return await Task.FromResult<EventData>(data);
        }

        private static async Task<EventData> GetEventHubEventDataAsync(string contentType, string typeName, byte[] message)
        {
            EventData data = new(message);
            data.Properties.Add("PassedBy", typeName);
            data.ContentType = contentType;
            return await Task.FromResult<EventData>(data);
        }

        /// <summary>
        /// Disposes the channel.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected async void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;

                try
                {
                    storage = null;

                    if (sender != null)
                    {
                        await sender.DisposeAsync();
                    }

                    if (processor != null)
                    {
                        processor.StopProcessing();
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "{Name}-{Id} event hub fault disposing.", Name, Id);
                }

                CloseAsync().GetAwaiter();
                logger?.LogInformation("{Name}-{Id} channel disposed.", Name, Id);
            }
        }
    }
}
