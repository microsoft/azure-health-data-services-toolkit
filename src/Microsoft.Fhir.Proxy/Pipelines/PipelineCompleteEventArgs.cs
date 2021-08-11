using System;

namespace Microsoft.Fhir.Proxy.Pipelines
{
    public class PipelineCompleteEventArgs : EventArgs
    {
        public PipelineCompleteEventArgs(string id, string name, OperationContext context)
        {
            Id = id;
            Name = name;
            Context = context;
        }

        public string Name { get; private set; }

        public string Id { get; private set; }

        public OperationContext Context { get; private set; }
    }
}
