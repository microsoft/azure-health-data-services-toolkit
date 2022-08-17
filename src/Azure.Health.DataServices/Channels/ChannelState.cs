namespace Azure.Health.DataServices.Channels
{
    /// <summary>
    /// An enumeration of channel states.
    /// </summary>
    public enum ChannelState
    {
        None,
        Connecting,
        Open,
        Closed,
        ClosedReceived,
        CloseSent,
        Aborted
    }
}
