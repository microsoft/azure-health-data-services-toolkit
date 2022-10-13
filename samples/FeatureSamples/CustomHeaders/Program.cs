// See https://aka.ms/new-console-template for more information
using CustomHeadersSample;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Specialized;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Configuration;
using System.Reflection;

// This sample does not use a fully fledged custom operation. It's only using the CustomHeaders component.
// In a complete custom operation, you would setup the headers like below. Instead of using MyService,
// you would add your Input filter and use that to control when the headers are modified.
// To invoke header modification, call IHttpCustomHeaderCollection.AppendAndReplace.

// Create builder to setup the custom header sample
var builder = Host.CreateDefaultBuilder(args);

// You can pull values from your configuration into custom headers. We don't have configuration in this basic sample, so using Environment instead.
string environmentName = Environment.GetEnvironmentVariable("AZURE_EnvironmentName") ?? "Development";

builder.ConfigureServices(services =>
{
    // Inform the pipeline that we are using Custom Headers
    services.UseCustomHeaders();

    // Static headers are always added
    services.AddCustomHeader("X-ServiceSource", "CustomHeaderTest", CustomHeaderType.RequestStatic);
    services.AddCustomHeader("X-EnvironmentName", environmentName, CustomHeaderType.RequestStatic);

    // Request headers are only added if the headers exists on the request
    services.AddCustomHeader("X-Custom-Location", "X-FHIR-Location", CustomHeaderType.RequestMatch);
    services.AddCustomHeader("X-Custom-Geo", "X-FHIR-Geography", CustomHeaderType.RequestMatch);

    // The "name" claim will be extracted and mapped to the "X-MS-Test" header
    services.AddCustomHeader("X-MS-TEST", "name", CustomHeaderType.RequestIdentity);
    services.AddSingleton<IMyService, MyService>();
});

// Build the builder for testing but don't start it since we don't have a full custom operation.
var app = builder.Build();

// Tests the sample
TestSample();

// Tests the above custom header operations.
void TestSample()
{
    HttpRequestMessage request = new();

    // This header will be replaces by the corresponding CustomerHeaderType.Request
    request.Headers.Add("X-Custom-Location", "Hospital");

    // This header won't appear in the output since it doesn't exist on the request
    // request.Headers.Add("X-Custom-Geo", "MidWest");

    // Sample token to pull the name claim
    string jwt = File.ReadAllText("jwttest.txt");
    request.Headers.Add("Authorization", $"Bearer {jwt}");

    NameValueCollection headers;
    using (var serviceScope = app.Services.CreateScope())
    {
        IMyService myservice = serviceScope.ServiceProvider.GetRequiredService<IMyService>();

        // This is what invokes the haeder replacement. You would usually do this in an input filter.
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