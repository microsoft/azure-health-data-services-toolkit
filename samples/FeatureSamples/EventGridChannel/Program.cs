// See https://aka.ms/new-console-template for more information
using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Configuration;
using Microsoft.AzureHealth.DataServices.Pipelines;
using EventGridChannelSample;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text;

// Sets up configuration either from dotnet secrets or environment variables (that start with AZURE_)
// Here we just are binding to an object that we can use later in our configuration/testing. For an example of 
// configuration via dependency injection, check out the Quickstart.
IConfigurationBuilder configBuilder = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
    .AddEnvironmentVariables("AZURE_");
IConfigurationRoot root = configBuilder.Build();
EventGridChannelConfig config = new();
root.Bind(config);

// Configures the custom operation.
IHostBuilder builder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddLogging();

        // This creates the custom operation pipeline.
        services.UseWebPipeline();

        // Adds Event Grid as the first input channel. Azure Storage is a backing store for events that are
        // too large for Event Grid.
        services.AddInputChannel<EventGridChannelOptions>(typeof(EventGridChannel), options =>
        {
            options.Subject = config.Subject;
            options.AccessKey = config.AccessKey;
            options.ExecutionStatusType = StatusType.Normal;
            options.DataVersion = config.DataVersion;
            options.EventType = config.EventType;
            options.FallbackStorageConnectionString = config.FallbackStorageConnectionString;
            options.FallbackStorageContainer = config.FallbackStorageContainer;
            options.TopicUriString = config.TopicUriString;
        });

        AzureQueueConfig queueConfig = new() { ConnectionString = config.FallbackStorageConnectionString, QueueName = config.QueueName };
        services.AddSingleton(queueConfig);
        services.AddSingleton<IPipelineService, PipelineService>();
    });

var app = builder.Build();
app.RunAsync().GetAwaiter();

// Get our wrapper for the pipeline service. This service allows us to use our pipeline for sending data to Event Grid in a pipeline
// then we have an event which we'll use for testing to show that data made it to our Event Grid.
IPipelineService myservice = app.Services.GetRequiredService<IPipelineService>();
myservice.OnReceive += (s, e) =>
{
    string base64 = Encoding.UTF8.GetString(e.Message.Data.ToArray()).Trim('"');
    string text = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(text);
    Console.ResetColor();
};

// Send a request to our pipeline to show data going to the Event Grid.
HttpRequestMessage request = new(HttpMethod.Post, "https://example.org");
request.Content = new StringContent("hi!");
request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

await myservice.ExecuteAsync(request);
