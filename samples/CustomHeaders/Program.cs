// See https://aka.ms/new-console-template for more information
using CustomHeadersSample;
using Azure.Health.DataServices.Clients.Headers;
using Azure.Health.DataServices.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Specialized;

IHostBuilder builder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.UseCustomHeaders();
        services.AddCustomHeader("MyHeaderName1", "MyHeaderName1", CustomHeaderType.Static);
        services.AddCustomHeader("MyHeaderName2", "MyHeaderName2", CustomHeaderType.Static);
        services.AddSingleton<IMyService, MyService>();
    });

var app = builder.Build();
app.RunAsync();

HttpRequestMessage request = new();

IMyService myservice = app.Services.GetRequiredService<IMyService>();
NameValueCollection headers = myservice.GetCustomHeaders(request);
foreach (string name in headers)
    foreach (string value in headers.GetValues(name))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{name}-{value}");
    }
Console.ResetColor();


