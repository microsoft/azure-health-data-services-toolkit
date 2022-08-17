// See https://aka.ms/new-console-template for more information
using Azure.Health.DataServices.Channels;
using Azure.Health.DataServices.Configuration;
using EventHubChannelSample;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;


IConfigurationBuilder configBuilder = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
    .AddEnvironmentVariables("AZURE");
IConfigurationRoot root = configBuilder.Build();
MyServiceConfig config = new();
root.Bind(config);


IHostBuilder builder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {   
        services.AddLogging();
        services.UseWebPipeline();       

        services.AddOutputChannel<EventHubChannelOptions>(typeof(EventHubChannel), options =>
        {
            options.Sku = EventHubSkuType.Basic;
            options.ConnectionString = config.ConnectionString;
            options.HubName = config.HubName;
            options.ExecutionStatusType = config.ExecutionStatusType;
            options.FallbackStorageConnectionString = config.FallbackStorageConnectionString;
            options.FallbackStorageContainer = config.FallbackStorageContainer;
            options.ProcessorStorageContainer = config.ProcessorStorageContainer;
        });

        services.AddSingleton<MyServiceConfig>(config);
        services.AddSingleton<IMyService, MyService>();
    });

var app = builder.Build();
app.RunAsync().GetAwaiter();

HttpRequestMessage request = new();

IMyService myservice = app.Services.GetRequiredService<IMyService>();
myservice.OnReceive += (_, e) =>
{
    string msg = Encoding.UTF8.GetString(e.Message);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Message Received !!!");
    Console.WriteLine(msg);
    Console.ResetColor();
};

Console.WriteLine("waiting to receive message from Event Hub...");
string text = "hello from event hub";
HttpRequestMessage message = new(HttpMethod.Post, "http://example.org/");
message.Content = new StringContent("hello from event hub");
message.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
message.Content.Headers.ContentLength = text.Length;

await myservice.SendMessageAsync(message);
await Task.Delay(10000);

