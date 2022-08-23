// See https://aka.ms/new-console-template for more information
using CustomHeadersSample;
using Azure.Health.DataServices.Clients.Headers;
using Azure.Health.DataServices.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Specialized;
using Azure.Health.DataServices.Pipelines;
using Microsoft.Extensions.Configuration;
using System.Reflection;

CustomHeaderConfig config = new CustomHeaderConfig()
{
    // This will be overridden by local.settings.json and/or environment variables
    EnvironmentName = "Default"
};

var app = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((hostingContext, configuration) =>
    {
        // Load configuration (w/ EnvironmentName)
        configuration
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .AddEnvironmentVariables("AZURE_");

        IConfigurationRoot configurationRoot = configuration.Build();
        configurationRoot.Bind(config);

    })
    .ConfigureServices(services =>
    {
        // Inform the pipeline that we are using Custom Headers
        services.UseCustomHeaders();

        // Static headers are always added
        services.AddCustomHeader("X-ServiceSource", "CustomHeaderTest", CustomHeaderType.Static);
        services.AddCustomHeader("X-EnvironmentName", config.EnvironmentName, CustomHeaderType.Static);

        // Request headers are only added if the headers exists on the request
        services.AddCustomHeader("X-Custom-Location", "X-FHIR-Location", CustomHeaderType.Request);
        services.AddCustomHeader("X-Custom-Geo", "X-FHIR-Geography", CustomHeaderType.Request);

        // The "name" claim will be extracted and mapped to the "X-MS-Test" header
        services.AddCustomHeader("X-MS-TEST", "name", CustomHeaderType.Identity);
        services.AddSingleton<IMyService, MyService>();
    })
    .Build();

//app.RunAsync();

// Tests the sample
TestSample();

void TestSample()
{
    HttpRequestMessage request = new();

    request.Headers.Add("X-Custom-Location", "Hospital");

    // This header won't appear in the output since it doesn't exist on the request
    // request.Headers.Add("X-Custom-Geo", "MidWest");

    // Sample 
    string jwt = File.ReadAllText("jwttest.txt");
    request.Headers.Add("Authorization", $"Bearer {jwt}");

    NameValueCollection headers;
    using (var serviceScope = app.Services.CreateScope())
    {
        IMyService myservice = serviceScope.ServiceProvider.GetRequiredService<IMyService>();
        headers = myservice.GetCustomHeaders(request);
    }

    foreach (string name in headers)
        foreach (string value in headers.GetValues(name))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{name} : {value}");
        }

    Console.ResetColor();
}