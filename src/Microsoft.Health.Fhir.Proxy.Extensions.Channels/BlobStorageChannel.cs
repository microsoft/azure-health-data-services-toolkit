using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Extensions.Channels.Configuration;
using Microsoft.Health.Fhir.Proxy.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Extensions.Channels
{
    public class BlobStorageChannel : IChannel
    {
        public BlobStorageChannel(BlobStorageConfig config, ILogger logger = null)
        {
            this.logger = logger;
            Id = Guid.NewGuid().ToString();
            storage = new StorageBlob(config.BlobStorageChannelConnectionString, config.InitialTransferSize, config.MaxConcurrency, config.MaxTransferSize, logger);
            blobContainer = config.BlobStorageChannelContainer;
        }

        private StorageBlob storage;
        private readonly string blobContainer;
        private bool disposed;
        private ChannelState state;
        private readonly ILogger logger;

        public string Id { get; private set; }

        public string Name => "BlobStorageChannel";

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
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, null));
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
            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, Name, null));
            logger?.LogInformation($"{Name}-{Id} channel opened.");
            await Task.CompletedTask;
        }

        public async Task ReceiveAsync()
        {
            //blob channel does not receive.
            await Task.CompletedTask;
        }

        /// <summary>
        /// Upload a blob to storage.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="items"></param>
        /// <returns>Task</returns>
        /// <remarks>The params object must be in the following order:
        /// (i) string blob name; if omitted a random name is used with .json extension.
        /// (ii) string container name; if omitted default is container from BlobConfig
        /// (iii) string content type; if omitted the default is "application/json"
        /// (iv) Blob Type; if omitted the default is Block
        /// (v) IDictionary<string,string> metadata, which can be null to omit.
        /// (vi) AccessTier tier, which can be null to use default.
        /// (vii) BlobRequestConditions conditions, which can be null to omit.
        /// (viii) CancellationToken</remarks>
        public async Task SendAsync(byte[] message, params object[] items)
        {
            try
            {
                items[0] ??= $"{Guid.NewGuid()}T{DateTime.UtcNow:HH-MM-ss-fffff}.json";
                items[1] ??= blobContainer;
                items[2] ??= "application/json";
                items[3] ??= BlobType.Block.ToString();

                string blobName = (string)items[0];
                string container = (string)items[1];
                string contentType = (string)items[2];
                BlobType type = (BlobType)items[3];
                IDictionary<string, string> metadata = items[4] != null ? (Dictionary<string, string>)items[4] : null;
                BlobRequestConditions conditions = items[5] != null ? (BlobRequestConditions)items[5] : null;
                CancellationToken token = items[6] != null ? (CancellationToken)items[6] : CancellationToken.None;

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

                logger?.LogInformation($"{Name}-{Id} channel wrote blob.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message, $"{Name}-{Id} channel error writing blob.");
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, ex));
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
                CloseAsync().GetAwaiter();
                logger?.LogInformation($"{Name}-{Id} channel disposed.");
            }
        }
    }
}
