using System;

namespace Azure.Health.DataServices.Channels
{
    /// <summary>
    /// Events args for channel error events.
    /// </summary>
    public class ChannelErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Creates an instance of ChannelErrorEventArgs.
        /// </summary>
        /// <param name="id">Unique ID of the channel instance.</param>
        /// <param name="name">Name of the channel.</param>
        /// <param name="error">Exception that occurred in the channel.</param>
        public ChannelErrorEventArgs(string id, string name, Exception error)
        {
            Id = id;
            Name = name;
            Error = error;
        }

        /// <summary>
        /// Gets channel instance ID.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets name of the channel.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets error that occurred in the channel.
        /// </summary>
        public Exception Error { get; private set; }
    }
}
