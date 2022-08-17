using Azure.Health.DataServices.Clients;
using Azure.Health.DataServices.Filters;
using Azure.Health.DataServices.Json;
using Azure.Health.DataServices.Pipelines;
using Azure.Health.DataServices.Protocol;
using Azure.Health.DataServices.Security;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;

using System.Threading.Tasks;

namespace Quickstart.Filters
{
    public class QuickstartFilter : IInputFilter
    {
        public QuickstartFilter(IOptions<QuickstartOptions> options, IAuthenticator authenticator, TelemetryClient telemetryClient = null, ILogger<QuickstartFilter> logger = null)
        {
            id = Guid.NewGuid().ToString();
            fhirServerUrl = options.Value.FhirServerUrl;
            retryDelaySeconds = options.Value.RetryDelaySeconds;
            maxRetryAttempts = options.Value.MaxRetryAttempts;
            status = options.Value.ExecutionStatusType;
            this.authenticator = authenticator;
            this.telemetryClient = telemetryClient;
            this.logger = logger;
        }

        private readonly StatusType status;
        private readonly string fhirServerUrl;
        private readonly double retryDelaySeconds;
        private readonly int maxRetryAttempts;
        private readonly IAuthenticator authenticator;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger logger;
        private readonly string id;

        public string Id => id;

        public string Name => "QuickstartFilter";

        public StatusType ExecutionStatusType => status;

        public event EventHandler<FilterErrorEventArgs> OnFilterError;

        public async Task<OperationContext> ExecuteAsync(OperationContext context)
        {
            // Ensure we have a valid context (useful with multiple filters)
            if (context == null || context.IsFatal)
            {
                return context;
            }

            // Do your custom operation here
            await Task.Delay(10);

            return context;
        }
    }
}
