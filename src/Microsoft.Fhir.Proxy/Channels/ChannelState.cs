namespace Microsoft.Health.Fhir.Proxy.Channels
{
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
