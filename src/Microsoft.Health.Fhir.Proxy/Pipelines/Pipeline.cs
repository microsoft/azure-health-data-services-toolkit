using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Filters;
using System;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    public abstract class Pipeline : IDisposable
    {
        public abstract event EventHandler<PipelineErrorEventArgs> OnError;

        public abstract event EventHandler<PipelineCompleteEventArgs> OnComplete;

        public virtual string Id { get; internal set; }

        public abstract string Name { get; }

        public virtual ChannelCollection Channels { get; internal set; }

        public virtual FilterCollection Filters { get; internal set; }

        public abstract Task<OperationContext> ExecuteAsync(OperationContext context);

        public abstract void Dispose();
    }
}
