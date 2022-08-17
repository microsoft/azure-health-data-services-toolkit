using Azure.Health.DataServices.Configuration;
using Azure.Health.DataServices.Pipelines;
using Azure.Health.DataServices.Security;
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

        // Load environment from the azd cli for local development.
        // This is included in the project output via the .csproj for debug configurations only (not for release).
        DotNetEnv.Env.Load();

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
        
        services.UseAuthenticator();
        services.UseAzureFunctionPipeline();
        services.AddInputFilter<QuickstartOptions>(typeof(QuickstartFilter), options =>
        {
            options.FhirServerUrl = config.FhirServerUrl;
            options.RetryDelaySeconds = 5.0;
            options.MaxRetryAttempts = 5;
            options.ExecutionStatusType = StatusType.Normal;
        });
    })
    .Build();

await host.RunAsync();