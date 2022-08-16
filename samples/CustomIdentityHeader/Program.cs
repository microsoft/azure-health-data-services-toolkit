using CustomIdentityHeaderSample;
using DataServices.Clients.Headers;
using DataServices.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Specialized;

IHostBuilder builder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.UseCustomHeaders();
        services.AddCustomHeader("X-MS-TEST", "name", CustomHeaderType.Identity);
        services.AddSingleton<IMyService, MyService>();
    });

var app = builder.Build();
app.RunAsync();

string jwt = File.ReadAllText("../../../jwttest.txt");
HttpRequestMessage request = new();
request.Headers.Add("Authorization", $"Bearer {jwt}");

IMyService myservice = app.Services.GetRequiredService<IMyService>();
NameValueCollection headers = myservice.GetHeaders(request);
foreach (string name in headers)
    foreach (string value in headers.GetValues(name))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{name} : {value}");
    }

Console.ResetColor();