using System;

namespace Microsoft.Health.Fhir.Proxy.Channels
{
    public class ChannelOpenEventArgs : EventArgs
    {
        public ChannelOpenEventArgs(string id, string name, dynamic message)
        {
            Id = id;
            Name = name;
            Message = message;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public dynamic Message { get; private set; }
    }
}
