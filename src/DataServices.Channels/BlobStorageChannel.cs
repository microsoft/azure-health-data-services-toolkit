using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Health.DataServices.Channels;
using Azure.Health.DataServices.Pipelines;
using Azure.Health.DataServices.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azure.Health.DataServices.Channels
{
    /// <summary>
    /// Channel that sends events to Azure blob storage.
    /// </summary>
    public class BlobStorageChannel : IInputChannel, IOutputChannel
    {
        /// <summary>
        /// Creates and instance of BlobStorageChannel.
        /// </summary>
        /// <param name="options">Options for sending to blob storage.</param>
        /// <param name="logger">ILogger</param>
        public BlobStorageChannel(IOptions<BlobStorageChannelOptions> options, ILogger<BlobStorageChannel> logger = null)
        {
            this.logger = logger;
            Id = Guid.NewGuid().ToString();
            storage = new StorageBlob(options.Value.ConnectionString, options.Value.InitialTransferSize, options.Value.MaxConcurrency, options.Value.MaxTransferSize, logger);
            blobContainer = options.Value.Container;
            statusType = options.Value.ExecutionStatusType;
        }

        private StorageBlob storage;
        private readonly string blobContainer;
        private bool disposed;
        private ChannelState state;
        private readonly ILogger logger;
        private readonly StatusType statusType;

        /// <summary>
        /// Gets the instance ID of the channel.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name of the channel, i.e., "BlobStorageChannel".
        /// </summary>
        public string Name => "BlobStorageChannel";


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
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, null));
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
            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, Name, null));
            logger?.LogInformation("{Name}-{Id} channel opened.", Name, Id);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Starts the recieve operation for the channel.
        /// </summary>
        /// <returns>Task</returns>
        /// <remarks>Receive operation is omitted without error for BlobStorageChannel.</remarks>
        public async Task ReceiveAsync()
        {
            //blob channel does not receive.
            await Task.CompletedTask;
        }

        /// <summary>
        /// Send a message to the channel and uploads a blob.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="items">Additional optional parameters.</param>
        /// <remarks>The params object must be in the following order 
        /// 1) string content type if omitted the default is json.
        /// 2) string blob name if omitted a random name is used with json extension.
        /// 3) string container name if omitted default is container from BlobConfig
        /// 4) Blob Type; if omitted the default is Block.
        /// 5) IDictionary&lt;string,string&gt; metadata, which can be null to omit.
        /// 6) AccessTier tier, which can be null to use default.
        /// 7) BlobRequestConditions conditions, which can be null to omit.
        /// 8) CancellationToken</remarks>
        /// <returns>Task</returns>

        public async Task SendAsync(byte[] message, params object[] items)
        {
            object[] parameters = new object[7];
            try
            {
                if (items == null)
                    items = new object[7];
                if (items.Length <= 7)
                    items.CopyTo(parameters, 0);

                parameters[0] ??= "application/json";
                parameters[1] ??= $"{Guid.NewGuid()}T{DateTime.UtcNow:HH-MM-ss-fffff}.json";
                parameters[2] ??= blobContainer;
                parameters[3] ??= BlobType.Block;

                string contentType = (string)parameters[0];
                string blobName = (string)parameters[1];
                string container = (string)parameters[2];
                BlobType type = (BlobType)parameters[3];
                IDictionary<string, string> metadata = parameters[4] != null ? (Dictionary<string, string>)parameters[4] : null;
                BlobRequestConditions conditions = parameters[5] != null ? (BlobRequestConditions)parameters[5] : null;
                CancellationToken token = parameters[6] != null ? (CancellationToken)parameters[6] : CancellationToken.None;

                await storage.CreateContainerIfNotExistsAsync(container);

                switch (type)
                {
                    case BlobType.Block:
                        await storage.WriteBlockBlobAsync(container, blobName, contentType, message, null, metadata, conditions, token);
                        break;
                    case BlobType.Append:
                        await storage.WriteAppendBlobAsync(container, blobName, contentType, message, null, null, metadata, conditions, token);
                        break;
                    default:
                        OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, new Exception("Undefined blob type.")));
                        break;
                }

                logger?.LogInformation("{Name}-{Id} channel wrote blob.", Name, Id);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "{Name}-{Id} channel error writing blob.", Name, Id);
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
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
                CloseAsync().GetAwaiter();
                logger?.LogInformation("{Name}-{Id} channel disposed.", Name, Id);
            }
        }
    }
}
