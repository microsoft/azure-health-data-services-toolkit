using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AzureHealth.DataServices.Bindings;
using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureHealth.DataServices.Pipelines
{
    /// <summary>
    /// A custom operation pipeline for a ASPNET Web API.
    /// </summary>
    public class WebPipeline : IPipeline<HttpRequestMessage, HttpResponseMessage>
    {
        /// <summary>
        /// Creates an instance of WebPipeline.
        /// </summary>
        /// <param name="inputFilters">Optional collection of input filters.</param>
        /// <param name="inputChannels">Optional cCollection of input channels.</param>
        /// <param name="binding">Optional binding. </param>
        /// <param name="outputFilters">Optional collection of output filters.</param>
        /// <param name="outputChannels">Optional collection of output channels.</param>
        /// <param name="telemetryClient">Optional application insights telemetry client.</param>
        /// <param name="logger">Optional ILogger.</param>
        public WebPipeline(IInputFilterCollection inputFilters = null, IInputChannelCollection inputChannels = null, IBinding binding = null, IOutputFilterCollection outputFilters = null, IOutputChannelCollection outputChannels = null, TelemetryClient telemetryClient = null, ILogger<WebPipeline> logger = null)
            : this("WebPipeline", Guid.NewGuid().ToString(), inputFilters, inputChannels, binding, outputFilters, outputChannels, telemetryClient, logger)
        {

        }

        internal WebPipeline(IInputFilterCollection inputFilters = null, IInputChannelCollection inputChannels = null, IBinding binding = null, IOutputFilterCollection outputFilters = null, IOutputChannelCollection outputChannels = null, TelemetryClient telemetryClient = null, ILogger<AzureFunctionPipeline> logger = null)
            : this("WebPipeline", Guid.NewGuid().ToString(), inputFilters, inputChannels, binding, outputFilters, outputChannels, telemetryClient, logger)
        {

        }

        internal WebPipeline(string name, string id, IInputFilterCollection inputFilters = null, IInputChannelCollection inputChannels = null, IBinding binding = null, IOutputFilterCollection outputFilters = null, IOutputChannelCollection outputChannels = null, TelemetryClient telemetryClient = null, ILogger logger = null)
        {
            this.name = name;
            this.id = id;

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

                foreach (var header in context.Headers.Where(x => x.HeaderType == Clients.Headers.CustomHeaderType.ResponseStatic))
                {
                    response.Headers.Add(header.Name, header.Value);
                }

                response.Content = !string.IsNullOrEmpty(context.ContentString) ? new StringContent(context.ContentString) : null;

                foreach (var header in context.Headers.Where(x => x.HeaderType == Clients.Headers.CustomHeaderType.ResponseStatic))
                {
                    response.Headers.Add(header.Name, header.Value);
                }

                logger?.LogInformation("Pipeline {Name}-{Id} complete {ExecutionTime}ms", Name, Id, TimeSpan.FromTicks(DateTime.Now.Ticks - startTicks).TotalMilliseconds);
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
            foreach (var filter in filters)
            {
                try
                {
                    if (ExecutionStatusCheck(filter.ExecutionStatusType, context))
                    {
                        logger?.LogInformation("Pipeline {Name}-{Id} filter {FilterName}-{FilterId} executed with status {Status}.", Name, Id, filter.Name, filter.Id, filter.ExecutionStatusType);
                        context = await filter.ExecuteAsync(context);
                    }
                    else
                    {
                        logger?.LogInformation("Pipeline {Name}-{Id} filter {FilterName}-{FilterId} not executed due to status {Status}.", Name, Id, filter.Name, filter.Id, filter.ExecutionStatusType);
                    }
                }
                catch (Exception ex)
                {
                    telemetryClient?.TrackException(ex);
                    logger?.LogError(ex, "Pipeline {Name}-{Id} channel {FilterName}-{FilterName} error.", Name, Id, filter.Name, filter.Id);
                    context.IsFatal = true;
                    context.StatusCode = HttpStatusCode.InternalServerError;
                    context.Error = ex;
                }
            }

            return context;
        }

        private async Task ExecuteChannelsAsync(IChannelCollection channels, OperationContext context)
        {
            foreach (var channel in channels)
            {
                try
                {
                    if (ExecutionStatusCheck(channel.ExecutionStatusType, context))
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

                        var contentType = context.Request.Content?.Headers.ContentType.ToString();
                        contentType ??= context.Request.Headers.Accept.ToString();

                        await channel.SendAsync(context.Content, new object[] { contentType });
                        logger?.LogInformation("Pipeline {Name}-{Id} channel {ChannelName}-{ChannelId} sent message.", Name, Id, channel.Name, channel.Id);
                    }
                    else
                    {
                        logger?.LogInformation("Pipeline {Name}-{Id} channel {ChannelName}-{ChannelId} not executed due to status {Status}.", Name, Id, channel.Name, channel.Id, channel.ExecutionStatusType);
                    }
                }
                catch (Exception ex)
                {
                    telemetryClient?.TrackException(ex);
                    logger?.LogError(ex, "Pipeline {Name}-{Id} channel {ChannelName}-{ChannelId} error.", Name, Id, channel.Name, channel.Id);
                    context.IsFatal = true;
                    context.StatusCode = HttpStatusCode.InternalServerError;
                    context.Error = ex;
                }
            }
        }


        private bool ExecutionStatusCheck(StatusType status, OperationContext context)
        {
            return status == StatusType.Fault && context.IsFatal ||
                    status == StatusType.Normal && !context.IsFatal ||
                    status == StatusType.Any;
        }

        #region error events

        private void Binding_OnComplete(object sender, BindingCompleteEventArgs e)
        {
            logger?.LogInformation("Pipeline {Name}-{Id} binding {BindingName}-{BindingId} completed.", Name, Id, e.Name, e.Id);
        }

        private void Binding_OnError(object sender, BindingErrorEventArgs e)
        {
            context.IsFatal = true;
            context.StatusCode = HttpStatusCode.InternalServerError;
            context.Error = e.Error;
            logger?.LogError(e.Error, "Pipeline {Name}-{Id} binding {BindingName}- {BindingId} error.", Name, Id, e.Name, e.Id);
            OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, e.Error));
        }

        private void OutputChannel_OnError(object sender, ChannelErrorEventArgs e)
        {
            logger?.LogError(e.Error, "Pipeline {Name}-{Id} output channel {ChannelName}- {ChannelId} error.", Name, Id, e.Name, e.Id);
            context.IsFatal = true;
            context.StatusCode = HttpStatusCode.InternalServerError;
            context.Error = e.Error;
            OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, e.Error));
        }

        private void InputChannel_OnError(object sender, ChannelErrorEventArgs e)
        {
            logger?.LogError(e.Error, "Pipeline {Name}-{Id} input channel {ChannelName}- {ChannelId} error.", Name, Id, e.Name, e.Id);
            context.IsFatal = true;
            context.StatusCode = HttpStatusCode.InternalServerError;
            context.Error = e.Error;
            OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, e.Error));
        }

        private void OutputFilter_OnFilterError(object sender, FilterErrorEventArgs e)
        {
            context.IsFatal = true;
            context.StatusCode = e.Code ?? HttpStatusCode.InternalServerError;
            context.Error = e.Error;
            logger?.LogError(e.Error, "Pipeline {Name}-{Id} output filter {ChannelName}-{ChannelId} error.", Name, Id, e.Name, e.Id);
            OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, e.Error));
        }

        private void InputFilter_OnFilterError(object sender, FilterErrorEventArgs e)
        {
            context.IsFatal = true;
            context.StatusCode = e.Code ?? HttpStatusCode.InternalServerError;
            context.Error = e.Error;
            logger?.LogError(e.Error, "Pipeline {Name}-{Id} input filter {ChannelName}-{ChannelId} error.", Name, Id, e.Name, e.Id);
            OnError?.Invoke(this, new PipelineErrorEventArgs(Id, Name, e.Error));
        }

        #endregion
    }
}
