using System;

namespace Microsoft.Health.Fhir.Proxy.Channels
{
    public class ChannelStateEventArgs : EventArgs
    {
        public ChannelStateEventArgs(string id, ChannelState state)
        {
            Id = id;
            State = state;
        }

        public string Id { get; private set; }

        public ChannelState State { get; private set; }
    }
}
