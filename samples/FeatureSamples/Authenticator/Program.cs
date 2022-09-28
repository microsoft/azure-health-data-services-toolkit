// See https://aka.ms/new-console-template for more information about console apps without a main function
using AuthenticatorSample;
using Microsoft.AzureHealth.DataServices.Configuration;
using Microsoft.AzureHealth.DataServices.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

// This loads our configuration first from dotnet user secrets and then
// environment variables. They must be prefixed with "AZURE_" before the name
// of the configuration variable in MyConfig.cs.
IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .AddEnvironmentVariables("AZURE_");

IConfigurationRoot root = configBuilder.Build();
MyConfig config = new();
root.Bind(config);

IHostBuilder builder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        // This sets up our Authenticator for getting access tokens for Azure resources. 
        // We have logic here to "UseAuthenticator" with no parameters if the application
        // does not have a service principal defined. This will use DefaultAzureCredential
        // to try to authenticate using the environment. If the proper configuration is 
        // passed in, we will use a service principal.

        if (string.IsNullOrEmpty(config.ClientId) || string.IsNullOrEmpty(config.ClientSecret) || string.IsNullOrEmpty(config.TenantId))
        {
            services.UseAuthenticator();
        }
        else
        {
            services.UseAuthenticator(options =>
            {
                options.CredentialType = ClientCredentialType.ClientSecret;
                options.ClientId = config.ClientId;
                options.ClientSecret = config.ClientSecret;
                options.TenantId = config.TenantId;
            });
        }
        
        if (string.IsNullOrEmpty(config.FhirServerUrl))
        {
            throw new Exception("The Fhir Server URL must be configured to run this sample");
        }
        
        // This is how you would add a service to your application with configuration scoped to that service.
        services.UseMyService(options =>
        {
            options.FhirServerUrl = config.FhirServerUrl;
        });
    });

// Build and start the app
var app = builder.Build();
app.RunAsync().GetAwaiter();

// Get and output the access token. You can decode this for details at http://jwt.ms
IMyService myservice = app.Services.GetRequiredService<IMyService>();
string accessToken = await myservice.GetTokenAsync();
Console.WriteLine(accessToken);