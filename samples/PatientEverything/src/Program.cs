using DataServices.Configuration;
using DataServices.Pipelines;
using DataServices.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PatientEverything.Configuration;
using PatientEverything.Filters;
using System.Reflection;
using System.Threading.Tasks;

namespace PatientEverything
{
    public class Program
    {
        private static MyServiceConfig config;
        public static async Task Main()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new MyServiceConfig();
            root.Bind(config);

            using IHost host = CreateHostBuilder().Build();
            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args = null) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.UseAppInsightsLogging(config.InstrumentationKey, LogLevel.Information);
                    services.UseTelemetry(config.InstrumentationKey);
                    services.UseAuthenticator(options =>
                    {
                        options.CredentialType = ClientCredentialType.ClientSecret;
                        options.ClientId = config.ClientId;
                        options.ClientSecret = config.ClientSecret;
                        options.TenantId = config.TenantId;
                    });
                    services.UseAzureFunctionPipeline();
                    services.AddInputFilter<PatientEverythingOptions>(typeof(PatientEverythingFilter), options =>
                    {
                        options.FhirServerUrl = config.FhirServerUrl;
                        options.PageSize = 100;
                        options.PageSize = 1000;
                        options.RetryDelaySeconds = 5.0;
                        options.MaxRetryAttempts = 5;
                        options.ExecutionStatusType = StatusType.Normal;
                    });
                });
    }
}