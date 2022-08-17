
using CustomRequestHeadersSample;
using Azure.Health.DataServices.Clients.Headers;
using Azure.Health.DataServices.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Specialized;


IHostBuilder builder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.UseCustomHeaders();
        services.AddCustomHeader("X-Custom-Location", "X-FHIR-Location", CustomHeaderType.Request);
        services.AddCustomHeader("X-Custom-Geo", "X-FHIR-Geography", CustomHeaderType.Request);
        services.AddScoped<IMyService, MyService>();
    });

var app = builder.Build();
app.RunAsync();

HttpRequestMessage request = new();
request.Headers.Add("X-Custom-Location", "Hospital");
request.Headers.Add("X-Custom-Geo", "MidWest");

IMyService myservice = app.Services.GetRequiredService<IMyService>();
NameValueCollection headers = myservice.GetHeaders(request);

foreach (string name in headers)
    foreach (string value in headers.GetValues(name))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{name} : {value}");
    }

Console.ResetColor();
