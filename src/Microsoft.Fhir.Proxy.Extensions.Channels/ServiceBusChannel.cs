using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Fhir.Proxy.Channels;
using Microsoft.Fhir.Proxy.Extensions.Channels.Configuration;
using Microsoft.Fhir.Proxy.Storage;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Extensions.Channels
{
    public class ServiceBusChannel : IChannel
    {
        public ServiceBusChannel(ServiceBusSettings settings, ILogger logger = null)
        {
            this.settings = settings;
            this.logger = logger;
        }

        private ChannelState state;
        private readonly ILogger logger;
        private readonly ServiceBusSettings settings;
        private StorageBlob storage;
        private bool disposed;
        private ServiceBusClient client;
        private ServiceBusSender sender;
        private ServiceBusProcessor processor;

        public string Id { get; private set; }

        public string Name => "ServiceBusChannel";

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

        public async Task OpenAsync()
        {
            storage = new StorageBlob(settings.BlobConnectionString);
            client = new(settings.ServiceBusConnectionString);
            sender = client.CreateSender(settings.Topic);

            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, Name, null));
            logger?.LogInformation($"{Name}-{Id} opened.");
            await Task.CompletedTask;
        }

        public async Task SendAsync(byte[] message, params object[] items)
        {
            try
            {
                string typeName = "Value";

                if ((settings.ServiceBusSku != ServiceBusSkuType.Premium && message.Length > 0x3E800) || (settings.ServiceBusSku == ServiceBusSkuType.Premium && message.Length > 0xF4240))
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
                logger?.LogError(ex, $"{Name}-{Id} error attempting to send message.");
            }
        }

        public async Task ReceiveAsync()
        {
            try
            {
                ServiceBusProcessorOptions options = new()
                {
                    AutoCompleteMessages = true,
                    ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
                };

                processor = client.CreateProcessor(settings.Topic, settings.Subscription, options);
                processor.ProcessErrorAsync += async (args) =>
                {
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, args.Exception));
                    await Task.CompletedTask;
                };

                processor.ProcessMessageAsync += async (args) =>
                {
                    ServiceBusReceivedMessage msg = args.Message;

                    if (msg.ApplicationProperties.ContainsKey("PassedBy") && (string)msg.ApplicationProperties["PassedBy"] == "Value")
                    {
                        OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, msg.Body.ToArray()));
                    }
                    else if (msg.ApplicationProperties.ContainsKey("PassedBy") && (string)msg.ApplicationProperties["PassedBy"] == "Reference")
                    {
                        var byRef = JsonConvert.DeserializeObject<EventDataByReference>(Encoding.UTF8.GetString(msg.Body.ToArray()));
                        var result = await storage.DownloadBlockBlobAsync(byRef.Container, byRef.Blob);
                        OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, result.Content.ToArray()));
                    }
                    else
                    {
                        logger?.LogWarning($"{Name}-{Id} with topic {settings.Topic} and subscription {settings.Subscription} does not understand message.");
                    }
                };

                await processor.StartProcessingAsync();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                logger?.LogError(ex, $"{Name}-{Id} error receiving messages.");
            }
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
                    logger.LogError(ex, $"{Name}-{Id} fault disposing.");
                }

                CloseAsync().GetAwaiter();
                logger?.LogInformation($"{Name}-{Id} disposed.");
            }
        }

        private async Task<string> WriteBlobAsync(string contentType, byte[] message)
        {
            string guid = Guid.NewGuid().ToString();
            string blob = $"{guid}T{DateTime.UtcNow:HH-MM-ss-fffff}";
            await storage.WriteBlockBlobAsync(settings.ServiceBusBlobContainer, blob, contentType, message);
            return blob;
        }

        private async Task<ServiceBusMessage> GetBlobEventDataAsync(string contentType, string blobName, string typeName)
        {
            EventDataByReference byref = new(settings.ServiceBusBlobContainer, blobName, contentType);
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
