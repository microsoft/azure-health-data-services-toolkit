using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Filters;
using System;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    /// <summary>
    /// A simple concrete pipeline.
    /// </summary>
    public class BasicPipeline : Pipeline
    {
        /// <summary>
        /// Creates an instance of BasicPipeline.
        /// </summary>
        /// <param name="logger">ILogger</param>
        public BasicPipeline(ILogger logger = null)
            : this(null, null, logger)
        {
        }

        /// <summary>
        /// Creates an instance of BasicPipeline.
        /// </summary>
        /// <param name="filters">Collection of filters.</param>
        /// <param name="channels">Collection of channels.</param>
        /// <param name="logger">ILogger</param>
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

        /// <summary>
        /// Get the name of the pipeline, i.e., "BasicPipeline".
        /// </summary>
        public override string Name => "BasicPipeline";

        /// <summary>
        /// An event that signals an exception in the pipeline.
        /// </summary>
        public override event EventHandler<PipelineErrorEventArgs> OnError;

        /// <summary>
        /// An event that signals the pipeline has completed.
        /// </summary>
        public override event EventHandler<PipelineCompleteEventArgs> OnComplete;

        /// <summary>
        /// Executes the pipeline.
        /// </summary>
        /// <param name="context">Operation context to execute.</param>
        /// <returns>OperationContext</returns>
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

        /// <summary>
        /// Disposes the pipeline.
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;
                this.context = null;
                this.Filters = null;

                if (this.Channels != null)
                {
                    foreach (var channel in this.Channels)
                    {
                        channel.Dispose();
                    }
                }

                this.Channels = null;
            }
        }




    }
}
