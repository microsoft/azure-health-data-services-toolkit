using System;

namespace Microsoft.Health.Fhir.Proxy.Channels
{
    /// <summary>
    /// Events args for channel open events.
    /// </summary>
    public class ChannelOpenEventArgs : EventArgs
    {
        /// <summary>
        /// Creates an instance of ChannelOpenEventArgs.
        /// </summary>
        /// <param name="id">Channel instance ID.</param>
        /// <param name="name">Channel name.</param>
        /// <param name="message">Message received when opening the channel.</param>
        public ChannelOpenEventArgs(string id, string name, dynamic message)
        {
            Id = id;
            Name = name;
            Message = message;
        }

        /// <summary>
        /// Gets the channel instance ID.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the channel name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the message (optional) when the channel is opened.
        /// </summary>
        public dynamic Message { get; private set; }
    }
}
