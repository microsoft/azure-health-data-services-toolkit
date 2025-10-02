﻿using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AzureHealth.DataServices.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Microsoft.AzureHealth.DataServices.Channels
{
    /// <summary>
    /// Channel that sends or receives events with Azure Event Hub.
    /// </summary>
    /// <remarks>The ProcessorStorageContainer storage container name must be configured to receive events.</remarks>
    public class EventHubChannel : IInputChannel, IOutputChannel
    {
        private readonly EventHubSkuType _sku;
        private readonly string _connectionString;
        private readonly string _namespace;
        private readonly string _hubName;
        private readonly string _processorContainer;
        private readonly string _fallbackStorageConnectionString;
        private readonly string _fallbackContainer;
        private readonly DefaultAzureCredential _credential;
        private readonly string _storageAccountName;
        private readonly ILogger _logger;
        private readonly StatusType _statusType;
        private ChannelState _state;
        private EventHubProducerClient _sender;
        private EventProcessorClient _processor;
        private StorageBlob _storage;
        private bool _disposed;

        /// <summary>
        /// Creates an instance of EventHubChannel for sending to an event hub.
        /// </summary>
        /// <param name="options">Send options.</param>
        /// <param name="logger">ILogger</param>
        public EventHubChannel(IOptions<EventHubChannelOptions> options, ILogger<EventHubChannel> logger = null)
        {
            _sku = options.Value.Sku;
            _connectionString = options.Value.ConnectionString;
            _namespace = options.Value.Namespace;
            _credential = options.Value.Credential;
            _hubName = options.Value.HubName;
            _statusType = options.Value.ExecutionStatusType;
            _fallbackStorageConnectionString = options.Value.FallbackStorageConnectionString;
            _fallbackContainer = options.Value.FallbackStorageContainer;
            _processorContainer = options.Value.ProcessorStorageContainer;
            _storageAccountName = options.Value.StorageAccountName;
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
        /// Gets the name of the channel, i.e., "EventHubChannel".
        /// </summary>
        public string Name => "EventHubChannel";

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
            if (!string.IsNullOrEmpty(_connectionString))
            {
                _sender = new EventHubProducerClient(_connectionString, _hubName);
            }
            else
            {
                _sender = new EventHubProducerClient(_namespace, _hubName, _credential);
            }

            if (!string.IsNullOrEmpty(_fallbackStorageConnectionString))
            {
                _storage = new StorageBlob(_fallbackStorageConnectionString);
            }
            else
            {
                _storage = new StorageBlob(new Uri($"https://{_storageAccountName}.blob.core.windows.net"), _credential);
            }

            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, Name, null));
            _logger?.LogInformation("{Name}-{Id} channel opened.", Name, Id);
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
        /// Starts the recieve operation for the channel.
        /// </summary>
        /// <returns>Task</returns>
        /// <remarks>Receive operation uses the EventHubProcessor.</remarks>
        public async Task ReceiveAsync()
        {
            if (string.IsNullOrEmpty(_processorContainer))
            {
                var exception = new EventHubChannelException("Requires blob storage container configured to receive event data.");
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, exception));
                throw exception;
            }

            try
            {
                string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

                var storageClient = !string.IsNullOrEmpty(_fallbackStorageConnectionString)
                    ? new BlobContainerClient(_fallbackStorageConnectionString, _processorContainer)
                    : new BlobContainerClient(new Uri($"https://{_storageAccountName}.blob.core.windows.net/{_processorContainer}"), _credential);

                _processor = !string.IsNullOrEmpty(_connectionString)
                    ? new EventProcessorClient(storageClient, consumerGroup, _connectionString, _hubName)
                    : new EventProcessorClient(storageClient, consumerGroup, _namespace, _hubName, _credential);

                _processor.ProcessEventAsync += async (args) =>
                {
                    await args.UpdateCheckpointAsync(args.CancellationToken);

                    if (args.Data.Properties.ContainsKey("PassedBy") && (string)args.Data.Properties["PassedBy"] == "Reference")
                    {
                        EventDataByReference byRef = JsonConvert.DeserializeObject<EventDataByReference>(Encoding.UTF8.GetString(args.Data.EventBody.ToArray()));
                        global::Azure.Storage.Blobs.Models.BlobDownloadResult result = await _storage.DownloadBlockBlobAsync(byRef.Container, byRef.Blob);
                        OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, result.Content.ToArray()));
                    }
                    else
                    {
                        OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, args.Data.EventBody.ToArray()));
                    }
                };

                _processor.ProcessErrorAsync += async (args) =>
                {
                    // log exception
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, args.Exception));
                    _logger?.LogError(args.Exception, "{Name}-{Id} event hub channel processor receive error.", Name, Id);
                    await Task.CompletedTask;
                };

                await _processor.StartProcessingAsync();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                _logger?.LogError(ex, "{Name}-{Id} event hub channel error receiving messages.", Name, Id);
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
                EventData data = null;
                string contentType = (string)items[0];

                if ((_sku == EventHubSkuType.Basic && message.Length > Constants.EventHubBasicSkuMaxMessageLength) || (_sku != EventHubSkuType.Basic && message.Length > Constants.EventHubNonBasicSkuMaxMessageLength))
                {
                    string blob = await WriteBlobAsync(contentType, message);
                    data = await GetBlobEventDataAsync(contentType, blob);
                }
                else
                {
                    data = new(message);
                    data.ContentType = contentType;
                }

                using EventDataBatch eventBatch = await _sender.CreateBatchAsync();
                if (eventBatch.TryAdd(data))
                {
                    await _sender.SendAsync(eventBatch);
                    _logger?.LogInformation("{Name}-{Id} event hub channel message sent.", Name, Id);
                }
                else
                {
                    Exception ex = new("Event hub could not send message.");
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                    _logger?.LogError(ex, "{Name}-{Id} event hub error sending message.", Name, Id);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
                _logger?.LogError(ex, "{Name}-{Id} event hub error attempting to send message.", Name, Id);
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
                        _processor.StopProcessing();
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "{Name}-{Id} event hub fault disposing.", Name, Id);
                }

                CloseAsync().GetAwaiter();
                _logger?.LogInformation("{Name}-{Id} channel disposed.", Name, Id);
            }
        }

        private static async Task<EventData> GetEventHubEventDataAsync(string contentType, byte[] message)
        {
            EventData data = new(message);
            data.Properties.Add("PassedBy", "Reference");
            data.ContentType = contentType;
            return await Task.FromResult<EventData>(data);
        }

        private async Task<string> WriteBlobAsync(string contentType, byte[] message)
        {
            if (_storage == null)
            {
                var exception = new EventHubChannelException("Requires blob storage configured to write.");
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, exception));
                throw exception;
            }

            string guid = Guid.NewGuid().ToString();
            string blob = $"{guid}T{DateTime.UtcNow:HH-MM-ss-fffff}";
            await _storage.WriteBlockBlobAsync(_fallbackContainer, blob, contentType, message);
            return blob;
        }

        private async Task<EventData> GetBlobEventDataAsync(string contentType, string blobName)
        {
            EventDataByReference byref = new(_fallbackContainer, blobName, contentType);
            string json = JsonConvert.SerializeObject(byref);
            EventData data = new(Encoding.UTF8.GetBytes(json));
            data.Properties.Add("PassedBy", "Reference");
            data.ContentType = contentType;
            return await Task.FromResult<EventData>(data);
        }
    }
}
