using Azure.Identity;
using Microsoft.AzureHealth.DataServices.Bindings;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quickstart.Configuration;
using Quickstart.Filters;

#pragma warning disable CA1852
internal static class Program
{
    private static async Task Main(string[] args)
    {
        MyServiceConfig config = new();

        var host = new HostBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureServices((context, services) =>
            {
                context.Configuration.Bind(config);

                if (config.InstrumentationKey != null)
                {
                    services.UseAppInsightsLogging(config.InstrumentationKey, LogLevel.Information);
                    services.UseTelemetry(config.InstrumentationKey);
                }

                // Setup custom headers for use in an Input Filter
                services.UseCustomHeaders();
                services.AddCustomHeader("X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST", "QuickstartCustomOperation", CustomHeaderType.RequestStatic);

                // Setup pipeline for Azure function
                services.UseWebPipeline();

                // Add our header modification as the first filter
                services.AddInputFilter(typeof(QuickstartFilter));

                // Add our binding to pass the call to the FHIR service
                services.AddBinding<RestBinding, RestBindingOptions>(options =>
                {
                    options.BaseAddress = config.FhirServerUrl;
                    options.Credential = new DefaultAzureCredential();
                });
            })
            .Build();

        await host.RunAsync();
    }
}
