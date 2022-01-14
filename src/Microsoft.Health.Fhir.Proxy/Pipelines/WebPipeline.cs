using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Fhir.Proxy.Bindings;
using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Filters;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Pipelines
{
    /// <summary>
    /// A custom operation pipeline for a ASPNET Web API.
    /// </summary>
    public class WebPipeline : IPipeline<HttpRequestMessage, HttpResponseMessage>
    {
        /// <summary>
        /// Creates an instance of WebPipeline.
        /// </summary>
        /// <param name="options">Pipeline options.</param>
        /// <param name="inputFilters">Optional collection of input filters.</param>
        /// <param name="inputChannels">Optional cCollection of input channels.</param>
        /// <param name="binding">Optional binding. </param>
        /// <param name="outputFilters">Optional collection of output filters.</param>
        /// <param name="outputChannels">Optional collection of output channels.</param>
        /// <param name="telemetryClient">Optional application insights telemetry client.</param>
        /// <param name="logger">Optional ILogger.</param>
        public WebPipeline(IOptions<PipelineOptions> options, IInputFilterCollection inputFilters = null, IInputChannelCollection inputChannels = null, IBinding binding = null, IOutputFilterCollection outputFilters = null, IOutputChannelCollection outputChannels = null, TelemetryClient telemetryClient = null, ILogger<WebPipeline> logger = null)
            : this("WebPipeline", Guid.NewGuid().ToString(), options, inputFilters, inputChannels, binding, outputFilters, outputChannels, telemetryClient, logger)
        {

        }

        internal WebPipeline(IOptions<PipelineOptions> options, IInputFilterCollection inputFilters = null, IInputChannelCollection inputChannels = null, IBinding binding = null, IOutputFilterCollection outputFilters = null, IOutputChannelCollection outputChannels = null, TelemetryClient telemetryClient = null, ILogger<AzureFunctionPipeline> logger = null)
            : this("WebPipeline", Guid.NewGuid().ToString(), options, inputFilters, inputChannels, binding, outputFilters, outputChannels, telemetryClient, logger)
        {

        }

        internal WebPipeline(string name, string id, IOptions<PipelineOptions> options, IInputFilterCollection inputFilters = null, IInputChannelCollection inputChannels = null, IBinding binding = null, IOutputFilterCollection outputFilters = null, IOutputChannelCollection outputChannels = null, TelemetryClient telemetryClient = null, ILogger logger = null)
        {
            this.name = name;
            this.id = id;

            faultOnChannelError = options.Value.FaultOnChannelError;
            this.inputFilters = inputFilters ?? new InputFilterCollection();
            this.outputFilters = outputFilters ?? new OutputFilterCollection();
            this.inputChannels = inputChannels ?? new InputChannelCollection();
            this.outputChannels = outputChannels ?? new OutputChannelCollection();
            this.binding = binding;
            this.telemetryClient = telemetryClient;
            this.logger = logger;

            foreach (var inputFilter in this.inputFilters)
            {
                inputFilter.OnFilterError += InputFilter_OnFilterError;
            }

            foreach (var outputFilter in this.outputFilters)
            {
                outputFilter.OnFilterError += OutputFilter_OnFilterError;
            }

            foreach (var inputChannel in this.inputChannels)
            {
                inputChannel.OnError += InputChannel_OnError;
            }

            foreach (var outputChannel in this.outputChannels)
            {
                outputChannel.OnError += OutputChannel_OnError;
            }

            if (binding != null)
            {
                binding.OnError += Binding_OnError;
                binding.OnComplete += Binding_OnComplete;
            }
        }


        private OperationContext context;
        private readonly string name;
        private readonly string id;
        private readonly bool faultOnChannelError;
        private readonly IInputFilterCollection inputFilters;
        private readonly IOutputFilterCollection outputFilters;
        private readonly IBinding binding;
        private readonly IInputChannelCollection inputChannels;
        private readonly IOutputChannelCollection outputChannels;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger logger;
        private long startTicks;

        /// <summary>
        /// Gets the name of the pipeline.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Gets the unique ID of the pipeline instance.
        /// </summary>
        public string Id => id;

        /// <summary>
        /// Signals an event that an error occurred in the pipeline.
        /// </summary>
        public event EventHandler<PipelineErrorEventArgs> OnError;

        /// <summary>
        /// Signals an event when the pipeline completes.
        /// </summary>
        public event EventHandler<PipelineCompleteEventArgs> OnComplete;

        /// <summary>
        /// Executes the pipeline and returns a response for the caller.
        /// </summary>
        /// <param name="request">Iniitial request from the Web service.</param>
        /// <returns>Response for Web service.</returns>
        public async Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage request)
        {
            startTicks = DateTime.Now.Ticks;

            try
            {
                context = new(request);

                context = await ExecuteFiltersAsync(inputFilters, context);

                await ExecuteChannelsAsync(inputChannels, context);

                context = await ExecuteBindingAsync(binding, context);

                context = await ExecuteFiltersAsync(outputFilters, context);

                await ExecuteChannelsAsync(outputChannels, context);
                context.StatusCode = !context.IsFatal && context.StatusCode == 0 ? HttpStatusCode.OK : context.StatusCode;
                HttpResponseMessage response = new(context.StatusCode);
                response.Content = !string.IsNullOrEmpty(context.ContentString) ? new StringContent(context.ContentString) : null;
                logger?.LogInformation("Pipeline {Name}-{Id} complete {}ms", Name, Id, TimeSpan.FromTicks(DateTime.Now.Ticks - startTicks).TotalMilliseconds);
                OnComplete?.Invoke(this, new PipelineCompleteEventArgs(Id, Name, context));
                return response;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Pipeline {Name}-{Id} error with fault response.", Name, Id);
                telemetryClient?.TrackException(ex);
                OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, ex));
            }

            logger?.LogWarning("Pipeline {Name}-{Id} fault return 503 response.", Name, Id);
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }

        private async Task<OperationContext> ExecuteBindingAsync(IBinding binding, OperationContext context)
        {
            if (context.IsFatal || binding == null)
                return context;

            return await binding.ExecuteAsync(context);
        }

        private async Task<OperationContext> ExecuteFiltersAsync(IFilterCollection filters, OperationContext context)
        {
            if (context.IsFatal)
                return context;

            foreach (var filter in filters)
            {
                if (!context.IsFatal)
                {
                    context = await filter.ExecuteAsync(context);
                }
            }

            return context;
        }

        private async Task ExecuteChannelsAsync(IChannelCollection channels, OperationContext context)
        {
            if (context.IsFatal)
                return;

            foreach (var channel in channels)
            {
                try
                {

                    if (channel.State == ChannelState.None)
                    {
                        logger?.LogInformation("Pipeline {Name}-{Id} opening channel {ChannelName}-{ChannelId} first time.", Name, Id, channel.Name, channel.Id);
                        await channel.OpenAsync();
                    }

                    if (channel.State != ChannelState.Open)
                    {
                        logger?.LogInformation("Pipeline {Name}-{Id} channel {ChannelName}-{ChannelId} is not open, will try to close and open.", Name, Id, channel.Name, channel.Id);
                        await channel.CloseAsync();
                        await channel.OpenAsync();
                    }

                    logger?.LogInformation("Pipeline {Name}-{Id} channel {ChannelName}-{ChannelId} is in state {State}.", Name, Id, channel.Name, channel.Id, channel.State);

                    await channel.SendAsync(context.Content);
                    logger?.LogInformation("Pipeline {Name}-{Id} channel {ChannelName}-{ChannelId} sent message.", Name, Id, channel.Name, channel.Id);
                }
                catch (Exception ex)
                {
                    telemetryClient?.TrackException(ex);
                    logger?.LogError(ex, "Pipeline {Name}-{Id} channel {ChannelName}-{ChannelId} error.", Name, Id, channel.Name, channel.Id);
                    if (faultOnChannelError)
                    {
                        context.IsFatal = true;
                        context.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                        context.Error = ex;
                    }
                }
            }
        }

        #region error events

        private void Binding_OnComplete(object sender, BindingCompleteEventArgs e)
        {
            logger?.LogInformation("Pipeline {Name}-{Id} binding {BindingName}-{BindingId} completed.", Name, Id, e.Name, e.Id);
        }

        private void Binding_OnError(object sender, BindingErrorEventArgs e)
        {
            context.IsFatal = true;
            context.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            context.Error = e.Error;
            logger?.LogError(e.Error, "Pipeline {Name}-{Id} binding {BindingName}- {BindingId} error.", Name, Id, e.Name, e.Id);
            OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, e.Error));
        }

        private void OutputChannel_OnError(object sender, ChannelErrorEventArgs e)
        {
            logger?.LogError(e.Error, "Pipeline {Name}-{Id} output channel {ChannelName}- {ChannelId} error.", Name, Id, e.Name, e.Id);

            if (faultOnChannelError)
            {
                context.IsFatal = true;
                context.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                context.Error = e.Error;
                OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, e.Error));
            }
        }

        private void InputChannel_OnError(object sender, ChannelErrorEventArgs e)
        {
            logger?.LogError(e.Error, "Pipeline {Name}-{Id} input channel {ChannelName}- {ChannelId} error.", Name, Id, e.Name, e.Id);

            if (faultOnChannelError)
            {
                context.IsFatal = true;
                context.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                context.Error = e.Error;
                OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, e.Error));
            }
        }

        private void OutputFilter_OnFilterError(object sender, FilterErrorEventArgs e)
        {
            context.IsFatal = true;
            context.StatusCode = e.Code ?? System.Net.HttpStatusCode.InternalServerError;
            context.Error = e.Error;
            logger?.LogError(e.Error, "Pipeline {Name}-{Id} output filter {ChannelName}-{ChannelId} error.", Name, Id, e.Name, e.Id);
            OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, e.Error));
        }

        private void InputFilter_OnFilterError(object sender, FilterErrorEventArgs e)
        {
            context.IsFatal = true;
            context.StatusCode = e.Code ?? System.Net.HttpStatusCode.InternalServerError;
            context.Error = e.Error;
            logger?.LogError(e.Error, "Pipeline {Name}-{Id} input filter {ChannelName}-{ChannelId} error.", Name, Id, e.Name, e.Id);
            OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, e.Error));
        }

        #endregion
    }
}
