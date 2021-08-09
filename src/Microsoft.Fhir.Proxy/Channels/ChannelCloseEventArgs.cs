using System;

namespace Microsoft.Fhir.Proxy.Channels
{
    public class ChannelCloseEventArgs : EventArgs
    {
        public ChannelCloseEventArgs(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }
    }
}
