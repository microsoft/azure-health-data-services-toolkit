namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    /// <summary>
    /// Requirement status for execution of a filter or channel.
    /// </summary>
    public enum StatusType
    {
        /// <summary>
        /// Execute regardless of whether OperationContext is faulted.
        /// </summary>
        Any,
        /// <summary>
        /// Execute only if OperationContext is not faulted.
        /// </summary>
        Normal,

        /// <summary>
        /// Execute only if OperationContext is faulted.
        /// </summary>
        Fault
    }
}
