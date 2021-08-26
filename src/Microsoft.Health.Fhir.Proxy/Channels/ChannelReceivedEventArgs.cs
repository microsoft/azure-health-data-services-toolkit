using System;
using System.Collections.Generic;

namespace Microsoft.Health.Fhir.Proxy.Channels
{
    public class ChannelReceivedEventArgs : EventArgs
    {
        public ChannelReceivedEventArgs(string id, string name, byte[] message)
            : this(id, name, message, null)
        {
        }

        public ChannelReceivedEventArgs(string id, string name, byte[] message,
            IEnumerable<KeyValuePair<string, string>> properties)
        {
            Id = id;
            Name = name;
            Message = message;
            Properties = properties;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public byte[] Message { get; private set; }

        public IEnumerable<KeyValuePair<string, string>> Properties { get; private set; }
    }
}
