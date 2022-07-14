using System;
using System.Collections.Generic;

namespace DataServices.Channels
{
    /// <summary>
    /// Events args for channel receive events.
    /// </summary>
    public class ChannelReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates an instance of ChannelReceivedEventArgs.
        /// </summary>
        /// <param name="id">Channel instance ID.</param>
        /// <param name="name">Channel name.</param>
        /// <param name="message">Message received by the channel.</param>
        public ChannelReceivedEventArgs(string id, string name, byte[] message)
            : this(id, name, message, null)
        {
        }

        /// <summary>
        /// Creates an instance of ChannelReceivedEventArgs.
        /// </summary>
        /// <param name="id">Channel instance ID.</param>
        /// <param name="name">Channel name.</param>
        /// <param name="message">Message received by the channel.</param>
        /// <param name="properties">Additional properties to be passed.</param>
        public ChannelReceivedEventArgs(string id, string name, byte[] message,
            IEnumerable<KeyValuePair<string, string>> properties)
        {
            Id = id;
            Name = name;
            Message = message;
            Properties = properties;
        }

        /// <summary>
        /// Gets channel instance ID.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets channel name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Message received by the channel.
        /// </summary>
        public byte[] Message { get; private set; }

        /// <summary>
        /// Gets additional properties passed on the channel receive.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Properties { get; private set; }
    }
}
