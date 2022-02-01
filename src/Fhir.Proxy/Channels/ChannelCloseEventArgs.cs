using System;

namespace Fhir.Proxy.Channels
{
    /// <summary>
    /// Event args for Channel close event.
    /// </summary>
    public class ChannelCloseEventArgs : EventArgs
    {
        /// <summary>
        /// Creates an instance of ChannelCloseEventArgs.
        /// </summary>
        /// <param name="id">Channel instance ID.</param>
        /// <param name="name">Name of the channel.</param>
        public ChannelCloseEventArgs(string id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Gets the channel instance ID.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        public string Name { get; private set; }
    }
}
