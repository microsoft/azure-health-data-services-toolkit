namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    /// <summary>
    /// Options for a pipeline
    /// </summary>
    public class PipelineOptions
    {

        /// <summary>
        /// Gets or sets an indicator that determines if the pipeline should return a fault message to the caller when a channel fails (true) 
        /// or the message from the fhir server and/or output filters if a channel fails (false).
        /// </summary>
        public bool FaultOnChannelError { get; set; }
    }
}
