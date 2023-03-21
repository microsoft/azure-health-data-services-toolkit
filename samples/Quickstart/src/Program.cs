using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
using Microsoft.AzureHealth.DataServices.Bindings;
using Microsoft.AzureHealth.DataServices.Clients;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Configuration;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quickstart.Configuration;
using Quickstart.Filters;
using System.Reflection;

MyServiceConfig config = new MyServiceConfig();

using IHost host = new HostBuilder()
    .ConfigureAppConfiguration((hostingContext, configuration) =>
    {
        configuration.Sources.Clear();

        IHostEnvironment env = hostingContext.HostingEnvironment;

        configuration
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .AddEnvironmentVariables("AZURE_");

        IConfigurationRoot configurationRoot = configuration.Build();

        configurationRoot.Bind(config);

    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        if (config.InstrumentationKey != null)
        {
            services.UseAppInsightsLogging(config.InstrumentationKey, LogLevel.Information);
            services.UseTelemetry(config.InstrumentationKey);
        }

        // Setup custom headers for use in an Input Filter
        services.UseCustomHeaders();
        services.AddCustomHeader("X-MS-AZUREFHIR-AUDIT-USER-TOKEN-TEST", "QuickstartCustomOperation", CustomHeaderType.RequestStatic);

        
        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddGenericRestClient(new Uri(config.FhirServerUrl))
            .WithCredential(new DefaultAzureCredential(true))
            .ConfigureOptions(options =>
            {
                options.Retry.Mode = Azure.Core.RetryMode.Fixed;
                options.Retry.MaxRetries = 3;
                options.Retry.MaxDelay = TimeSpan.FromSeconds(120);
            });
        });
        // Setup pipeline for Azure function
        services.UseAzureFunctionPipeline();

        // Add our header modification as the first filter
        services.AddInputFilter(typeof(QuickstartFilter));

        // Add our binding to pass the call to the FHIR service
        services.AddRestBinding();
    })
    .Build();

await host.RunAsync();
