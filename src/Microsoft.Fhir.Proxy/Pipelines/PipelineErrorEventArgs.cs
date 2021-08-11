using System;

namespace Microsoft.Fhir.Proxy.Pipelines
{
    public class PipelineErrorEventArgs
    {
        public PipelineErrorEventArgs(string id, string name, Exception error)
        {
            Id = id;
            Name = name;
            Error = error;
        }

        public string Name { get; private set; }

        public string Id { get; private set; }

        public Exception Error { get; private set; }
    }
}
