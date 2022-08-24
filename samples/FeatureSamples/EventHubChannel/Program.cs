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

// Sets up configuration either from dotnet secrets or environment variables (that start with AZURE_)
// Here we just are binding to an object that we can use later in our configuration/testing. For an example of 
// configuration via dependency injection, check out the Quickstart.
IConfigurationBuilder configBuilder = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
    .AddEnvironmentVariables("AZURE_");
IConfigurationRoot root = configBuilder.Build();
MyServiceConfig config = new();
root.Bind(config);

// Configures the custom operation.
IHostBuilder builder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {   
        services.AddLogging();

        // This creates the custom operation pipeline.
        services.UseWebPipeline();

        // Adds Event Hubs as the first output channel. Azure Storage is a backing store for events that are
        // too large for Event Hubs.
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

// Get our wrapper for the pipeline service. This service allows us to use our pipeline for sending data to Event Grid in a pipeline
// then we have an event which we'll use for testing to show that data made it to our Event Grid.
IMyService myservice = app.Services.GetRequiredService<IMyService>();
myservice.OnReceive += (_, e) =>
{
    string msg = Encoding.UTF8.GetString(e.Message);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Message Received !!!");
    Console.WriteLine(msg);
    Console.ResetColor();
};

// Send a request to our pipeline to show data going to the Event Hubs.
Console.WriteLine("waiting to receive message from Event Hub...");
string text = "hello from event hub";
HttpRequestMessage message = new(HttpMethod.Post, "http://example.org/");
message.Content = new StringContent("hello from event hub");
message.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
message.Content.Headers.ContentLength = text.Length;

await myservice.SendMessageAsync(message);
await Task.Delay(10000);

