using System;

namespace Fhir.Proxy.Pipelines
{
    /// <summary>
    /// Event args for pipeline error events.
    /// </summary>
    public class PipelineErrorEventArgs
    {
        /// <summary>
        /// Creates an instance of PipelineErrorEventArgs.
        /// </summary>
        /// <param name="id">Instance ID of the pipeline.</param>
        /// <param name="name">Name of the pipeline.</param>
        /// <param name="error">Exception thrown in the pipeline.</param>
        public PipelineErrorEventArgs(string id, string name, Exception error)
        {
            Id = id;
            Name = name;
            Error = error;
        }

        /// <summary>
        /// Gets the name of the pipeline.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the instance ID of the pipeline.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Exception throw in the pipeline.
        /// </summary>
        public Exception Error { get; private set; }
    }
}
