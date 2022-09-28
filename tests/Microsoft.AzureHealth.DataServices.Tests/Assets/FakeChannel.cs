using System;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Pipelines;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets
{
    public class FakeChannel : IInputChannel, IOutputChannel
    {
        public FakeChannel()
        {
            Id = Guid.NewGuid().ToString();
        }

        private string id;
        private ChannelState state;
        private bool disposed;

        public string Id
        {
            get { return id; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    OnError?.Invoke(this, new ChannelErrorEventArgs("NA", Name, null));
                }
                else
                {
                    id = value;
                }
            }
        }

        public string Name => "FakeChannel";

        public StatusType ExecutionStatusType => StatusType.Any;

        public bool IsAuthenticated => false;

        public bool IsEncrypted => false;

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
            State = ChannelState.Closed;
            OnClose?.Invoke(this, new ChannelCloseEventArgs(Id, Name));
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
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Name, null));
            await Task.CompletedTask;
        }

        public async Task SendAsync(byte[] message, params object[] items)
        {
            await Task.CompletedTask;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool dispose)
        {
            if (dispose && !disposed)
            {
                disposed = true;
                if (state != ChannelState.Closed)
                {
                    CloseAsync().GetAwaiter();
                }
            }
        }
    }
}
