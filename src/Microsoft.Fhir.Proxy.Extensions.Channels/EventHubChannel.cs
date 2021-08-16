using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
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
    public class EventHubChannel : IChannel
    {
        public EventHubChannel(EventHubSettings settings, ILogger logger = null)
        {
            this.settings = settings;
            this.logger = logger;
        }

        private ChannelState state;
        private EventHubProducerClient sender;
        private EventProcessorClient processor;
        private readonly EventHubSettings settings;
        private readonly ILogger logger;
        private StorageBlob storage;
        private bool disposed;

        public string Id { get; private set; }

        public string Name => "EventHubChannel";

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
            sender = new EventHubProducerClient(settings.EventHubConnectionString, settings.EventHubName);
            storage = new StorageBlob(settings.BlobConnectionString);

            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, Name, null));
            logger?.LogInformation($"{Name}-{Id} channel opened.");
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


        public async Task ReceiveAsync()
        {
            try
            {
                string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
                var storageClient = new BlobContainerClient(settings.BlobConnectionString, settings.EventHubProcessorContainer);
                processor = new EventProcessorClient(storageClient, consumerGroup, settings.EventHubConnectionString, settings.EventHubName);

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
                    logger?.LogError(args.Exception, $"{Name}-{Id} event hub channel processor receive error.");
                    await Task.CompletedTask;
                };

                await processor.StartProcessingAsync();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                logger?.LogError(ex, $"{Name}-{Id} event hub channel error receiving messages.");
            }
        }

        public async Task SendAsync(byte[] message, params object[] items)
        {
            try
            {
                string typeName = "Value";

                if ((settings.EventHubSku == EventHubSkuType.Basic && message.Length > 0x3E8) || (settings.EventHubSku != EventHubSkuType.Basic && message.Length > 0xF4240))
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
                    logger?.LogInformation($"{Name}-{Id} event hub channel message sent.");
                }
                else
                {
                    Exception ex = new("Event hub could not send message.");
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                    logger?.LogError(ex, $"{Name}-{Id} event hub error sending message.");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                logger?.LogError(ex, $"{Name}-{Id} event hub error attempting to send message.");
            }
        }

        private async Task<string> WriteBlobAsync(string contentType, byte[] message)
        {
            string guid = Guid.NewGuid().ToString();
            string blob = $"{guid}T{DateTime.UtcNow:HH-MM-ss-fffff}";
            await storage.WriteBlockBlobAsync(settings.BlobContainer, blob, contentType, message);
            return blob;
        }

        private async Task<EventData> GetBlobEventDataAsync(string contentType, string blobName, string typeName)
        {
            EventDataByReference byref = new(settings.BlobContainer, blobName, contentType);
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
                    logger.LogError(ex, $"{Name}-{Id} event hub fault disposing.");
                }

                CloseAsync().GetAwaiter();
                logger?.LogInformation($"{Name}-{Id} channel disposed.");
            }
        }
    }
}
