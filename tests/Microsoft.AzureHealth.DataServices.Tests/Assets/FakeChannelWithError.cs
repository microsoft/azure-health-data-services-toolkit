using System;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Pipelines;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets
{
    public class FakeChannelWithError : IChannel
    {
        private readonly Exception _error;
        private ChannelState _state;
        private bool _disposed;

        public FakeChannelWithError(Exception error)
        {
            Id = Guid.NewGuid().ToString();
            _error = error;
        }

        public event EventHandler<ChannelCloseEventArgs> OnClose;

        public event EventHandler<ChannelErrorEventArgs> OnError;

        public event EventHandler<ChannelOpenEventArgs> OnOpen;

        public event EventHandler<ChannelReceivedEventArgs> OnReceive;

        public event EventHandler<ChannelStateEventArgs> OnStateChange;

        public string Id { get; private set; }

        public string Name => "FakeChannel";

        public StatusType ExecutionStatusType => StatusType.Any;

        public bool IsAuthenticated => false;

        public bool IsEncrypted => false;

        public int Port => 0;

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
            OnError?.Invoke(this, new ChannelErrorEventArgs(Id, Name, _error));
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool dispose)
        {
            if (dispose && !_disposed)
            {
                _disposed = true;
                if (_state != ChannelState.Closed)
                {
                    CloseAsync().GetAwaiter();
                }
            }
        }
    }
}
