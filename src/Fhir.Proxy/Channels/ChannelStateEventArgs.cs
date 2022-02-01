using System;

namespace Fhir.Proxy.Channels
{
    /// <summary>
    /// Events args for channel state change events.
    /// </summary>
    public class ChannelStateEventArgs : EventArgs
    {
        /// <summary>
        /// Creates an instance of ChannelStateEventArgs.
        /// </summary>
        /// <param name="id">The channel instance ID.</param>
        /// <param name="state">The state of the channel.</param>
        public ChannelStateEventArgs(string id, ChannelState state)
        {
            Id = id;
            State = state;
        }

        /// <summary>
        /// Gets the ID of the channel instance.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the state of the channel.
        /// </summary>
        public ChannelState State { get; private set; }
    }
}
