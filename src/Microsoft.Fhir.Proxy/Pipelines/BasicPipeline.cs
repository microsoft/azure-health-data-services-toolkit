using Microsoft.Extensions.Logging;
using Microsoft.Fhir.Proxy.Channels;
using Microsoft.Fhir.Proxy.Filters;
using System;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Pipelines
{
    public class BasicPipeline : Pipeline
    {
        public BasicPipeline(ILogger logger = null)
            : this(null, null, logger)
        {
        }

        public BasicPipeline(FilterCollection filters, ChannelCollection channels, ILogger logger = null)
        {
            Id = Guid.NewGuid().ToString();
            Filters = filters;
            Channels = channels;
            this.logger = logger;
        }

        private readonly ILogger logger;
        private OperationContext context;
        private bool disposed;

        public override string Name => "BasicPipeline";

        public override event EventHandler<PipelineErrorEventArgs> OnError;
        public override event EventHandler<PipelineCompleteEventArgs> OnComplete;

        public override async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            if (context.IsFatal)
            {
                return context;
            }

            this.context = context;

            foreach (var filter in Filters)
            {
                filter.OnFilterError += Filter_OnFilterError;
                context = await filter.ExecuteAsync(context);
            }

            foreach (var channel in Channels)
            {
                channel.OnError += Channel_OnError;
                await channel.OpenAsync();
                await channel.SendAsync(context.Content);
            }

            OnComplete?.Invoke(this, new PipelineCompleteEventArgs(Id, Name, context));

            return context;
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            logger?.LogWarning($"{Name}-{Id} channel error in pipeline.");
            logger?.LogError(e.Error, $"{Name}-{Id} - Channel {e.Id} {e.Error.Message}");
            OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, e.Error));
        }

        private void Filter_OnFilterError(object sender, FilterErrorEventArgs e)
        {
            context.IsFatal = true;
            context.StatusCode = e.Code ?? System.Net.HttpStatusCode.InternalServerError;
            context.Error = e.Error;
            logger?.LogWarning($"{Name}-{Id} filter error in pipeline.");
            OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, e.Error));
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                this.context = null;
                this.Filters = null;
                this.Channels = null;
                disposed = true;
            }
        }




    }
}
