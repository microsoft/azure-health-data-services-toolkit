using System;

namespace Fhir.Proxy.Pipelines
{
    /// <summary>
    /// Event args for pipeline complete.
    /// </summary>
    public class PipelineCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Creates an instance of PipelineCompleteEventArgs.
        /// </summary>
        /// <param name="id">Instance ID of the pipeline.</param>
        /// <param name="name">Name of pipeline.</param>
        /// <param name="context">OperationContext</param>
        public PipelineCompleteEventArgs(string id, string name, OperationContext context)
        {
            Id = id;
            Name = name;
            Context = context;
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
        /// Gets the OperationContext.
        /// </summary>
        public OperationContext Context { get; private set; }
    }
}
