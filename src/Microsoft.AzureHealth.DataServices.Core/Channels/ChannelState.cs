namespace Microsoft.AzureHealth.DataServices.Channels
{
    /// <summary>
    /// An enumeration of channel states.
    /// </summary>
    public enum ChannelState
    {
        /// <summary>
        /// Channel state is not set.
        /// </summary>
        None,

        /// <summary>
        /// Channel is connecting.
        /// </summary>
        Connecting,

        /// <summary>
        /// Channnel is open.
        /// </summary>
        Open,

        /// <summary>
        /// Channel is closed.
        /// </summary>
        Closed,

        /// <summary>
        /// Channel received a close notification.
        /// </summary>
        ClosedReceived,

        /// <summary>
        /// Channel sent a close notification.
        /// </summary>
        CloseSent,

        /// <summary>
        /// Channel has aborted.
        /// </summary>
        Aborted,
    }
}
