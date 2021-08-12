using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Fhir.Proxy.Channels;
using Microsoft.Fhir.Proxy.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Extensions.Channels
{
    public class BlobStorageChannel : IChannel
    {
        public BlobStorageChannel(string connectionString, long? initialTransferSize = null, int? maxConcurrency = null, int? maxTransferSize = null, ILogger logger = null)
        {
            Id = Guid.NewGuid().ToString();
            storage = new StorageBlob(connectionString, initialTransferSize, maxConcurrency, maxTransferSize, logger);
        }

        private StorageBlob storage;
        private bool disposed;
        private ChannelState state;

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

        public Task AddMessageAsync(byte[] message)
        {
            throw new NotImplementedException();
        }

        public async Task CloseAsync()
        {
            if (State != ChannelState.Closed)
            {
                State = ChannelState.Closed;
                OnClose?.Invoke(this, new ChannelCloseEventArgs(Id, Name));
            }

            await Task.CompletedTask;
        }

        public async Task OpenAsync()
        {
            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, Name, null));
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
        /// <returns></returns>
        /// <remarks>The params object must be in the following order:
        /// 1. string container name
        /// 2. string blob name
        /// 3. string content type
        /// 4. Blob Type
        /// 5. IDictionary<string,string> metadata, which can be null to omit.
        /// 6. AccessTier tier, which can be null to use default.
        /// 7. BlobRequestConditions conditions, which can be null to omit.
        /// 8. CancellationToken cancellationToken</string></remarks>
        public async Task SendAsync(byte[] message, params object[] items)
        {
            try
            {
                string container = (string)items[0];
                string blobName = (string)items[1];
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
            }
            catch (Exception ex)
            {
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
            }
        }
    }
}
