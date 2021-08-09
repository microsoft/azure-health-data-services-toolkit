using System;

namespace Microsoft.Fhir.Proxy.Channels
{
    public class ChannelErrorEventArgs : EventArgs
    {
        public ChannelErrorEventArgs(string id, string name, Exception error)
        {
            Id = id;
            Name = name;
            Error = error;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public Exception Error { get; private set; }
    }
}
