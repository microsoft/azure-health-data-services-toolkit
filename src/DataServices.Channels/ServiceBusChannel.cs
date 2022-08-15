using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Health.DataServices.Channels;
using Azure.Health.DataServices.Pipelines;
using Azure.Health.DataServices.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Azure.Health.DataServices.Channels
{
    /// <summary>
    /// Channel that can send or receive data from Azure Service Bus.
    /// </summary>
    public class ServiceBusChannel : IInputChannel, IOutputChannel
    {
        /// <summary>
        /// Creates an instance of ServiceBusChannel for sending or receiving messages from service bus.
        /// </summary>
        /// <param name="options">Send options.</param>
        /// <param name="logger">ILogger</param>
        /// <remarks>The subscription option must be configured to receive events.</remarks>
        public ServiceBusChannel(IOptions<ServiceBusChannelOptions> options, ILogger<ServiceBusChannel> logger = null)
        {
            Id = Guid.NewGuid().ToString();
            sku = options.Value.Sku;
            connectionString = options.Value.ConnectionString;
            fallbackStorageConnectionString = options.Value.FallbackStorageConnectionString;
            storageContainer = options.Value.FallbackStorageContainer;
            topic = options.Value.Topic;
            subscription = options.Value.Subscription;
            queue = options.Value.Queue;
            statusType = options.Value.ExecutionStatusType;
            this.logger = logger;
        }


        private ChannelState state;
        private readonly ILogger logger;
        private readonly StatusType statusType;
        private readonly string connectionString;
        private readonly string fallbackStorageConnectionString;
        private readonly string storageContainer;
        private readonly string topic;
        private readonly string subscription;
        private readonly string queue;
        private readonly ServiceBusSkuType sku;
        private StorageBlob storage;
        private bool disposed;
        private ServiceBusClient client;
        private ServiceBusSender sender;
        private ServiceBusProcessor processor;

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
            storage = new StorageBlob(fallbackStorageConnectionString, null, 10, null, logger);
            client = new(connectionString);
            string resource = topic ?? queue;
            sender = client.CreateSender(resource);

            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, Name, null));
            logger?.LogInformation("{Name}-{Id} opened.", Name, Id);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Sends a message to a Service Bus topic if size &lt; SKU constraint; otherwise uses blob storage.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="items">Additional optional parameters, where required is the content type.</param>
        /// <returns></returns>
        public async Task SendAsync(byte[] message, params object[] items)
        {
            try
            {
                string typeName = "Value";

                if ((sku != ServiceBusSkuType.Premium && message.Length > Constants.ServiceBusNonPremiumSkuMaxMessageLength) || (sku == ServiceBusSkuType.Premium && message.Length > Constants.ServiceBusPremiumSkuMaxMessageLength))
                {
                    typeName = "Reference";
                }

                string contentType = (string)items[0];
                ServiceBusMessage data = null;

                if (typeName == "Reference")
                {
                    string blob = await WriteBlobAsync(contentType, message);
                    data = await GetBlobEventDataAsync(contentType, blob, typeName);
                }
                else
                {
                    data = await GetServiceBusEventDataAsync(contentType, typeName, message);
                }

                await sender.SendMessageAsync(data);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                logger?.LogError(ex, "{Name}-{Id} error attempting to send message.", Name, Id);
            }
        }

        /// <summary>
        /// Starts the recieve operation for the channel.
        /// </summary>
        /// <returns>Task</returns>
        /// <remarks>Receive operation and subscription in Service Bus.</remarks>
        public async Task ReceiveAsync()
        {

            if (string.IsNullOrEmpty(subscription) && string.IsNullOrEmpty(queue))
            {
                throw new InvalidOperationException("Neither queue or subscription configured to receive");
            }

            if (!string.IsNullOrEmpty(subscription) && !string.IsNullOrEmpty(queue))
            {
                throw new InvalidOperationException("Both queue and subscription cannot be configured to receive");
            }

            try
            {


                ServiceBusProcessorOptions options = new()
                {
                    AutoCompleteMessages = true,
                    ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
                };

                if (queue != null)
                {
                    processor = client.CreateProcessor(queue, options);
                    logger?.LogInformation("{Name}-{Id} queue {queue} receiving.", Name, Id, queue);
                }

                if (subscription != null)
                {
                    processor = client.CreateProcessor(topic, subscription, options);
                    logger?.LogInformation("{Name}-{Id} topic {topic} subscription {sub} receiving.", Name, Id, topic, subscription);
                }

                processor.ProcessErrorAsync += async (args) =>
                {
                    logger?.LogInformation("{Name}-{Id} received error in processor.", Name, Id);
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, args.Exception));
                    await Task.CompletedTask;
                };

                processor.ProcessMessageAsync += async (args) =>
                {
                    ServiceBusReceivedMessage msg = args.Message;
                    logger?.LogInformation("{Name}-{Id} message received.", Name, Id);

                    if (msg.ApplicationProperties.ContainsKey("PassedBy") && (string)msg.ApplicationProperties["PassedBy"] == "Value")
                    {
                        logger?.LogInformation("{Name}-{Id} processing subscription message.", Name, Id);
                        OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, msg.Body.ToArray()));
                    }
                    else if (msg.ApplicationProperties.ContainsKey("PassedBy") && (string)msg.ApplicationProperties["PassedBy"] == "Reference")
                    {
                        var byRef = JsonConvert.DeserializeObject<EventDataByReference>(Encoding.UTF8.GetString(msg.Body.ToArray()));
                        var result = await storage.ReadBlockBlobAsync(byRef.Container, byRef.Blob);
                        logger?.LogInformation("{Name}-{Id} processing large message from blob.", Name, Id);
                        OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, result));
                    }
                    else
                    {
                        logger?.LogWarning("{Name}-{Id} does not understand received message.", Name, Id);
                        OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, new Exception("Invalid parameters in processor.")));
                    }
                };

                await processor.StartProcessingAsync();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                logger?.LogError(ex, "{Name}-{Id} error receiving messages.", Name, Id);
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
                logger?.LogInformation("{Name}-{Id} channel closed.", Name, Id);
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
                        await processor.StopProcessingAsync();
                        await processor.DisposeAsync();
                    }

                    if (client != null)
                    {
                        await client.DisposeAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "{Name}-{Id} fault disposing.", Name, Id);
                }

                CloseAsync().GetAwaiter();
                logger?.LogInformation("{Name}-{Id} disposed.", Name, Id);
            }
        }

        private async Task<string> WriteBlobAsync(string contentType, byte[] message)
        {
            string guid = Guid.NewGuid().ToString();
            string blob = $"{guid}T{DateTime.UtcNow:HH-MM-ss-fffff}";
            await storage.WriteBlockBlobAsync(storageContainer, blob, contentType, message);
            return blob;
        }

        private async Task<ServiceBusMessage> GetBlobEventDataAsync(string contentType, string blobName, string typeName)
        {
            EventDataByReference byref = new(storageContainer, blobName, contentType);
            string json = JsonConvert.SerializeObject(byref);
            ServiceBusMessage data = new(Encoding.UTF8.GetBytes(json));
            data.ApplicationProperties.Add("PassedBy", typeName);
            data.ContentType = contentType;
            return await Task.FromResult<ServiceBusMessage>(data);
        }

        private static async Task<ServiceBusMessage> GetServiceBusEventDataAsync(string contentType, string typeName, byte[] message)
        {
            ServiceBusMessage data = new(message);
            data.ApplicationProperties.Add("PassedBy", typeName);
            data.ContentType = contentType;
            return await Task.FromResult<ServiceBusMessage>(data);
        }
    }
}
