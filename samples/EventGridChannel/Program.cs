// See https://aka.ms/new-console-template for more information


using Azure.Messaging.EventGrid;
using Azure.Health.DataServices.Channels;
using Azure.Health.DataServices.Configuration;
using Azure.Health.DataServices.Pipelines;
using EventGridChannelSample;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text;

IConfigurationBuilder configBuilder = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
    .AddEnvironmentVariables("AZURE_");
IConfigurationRoot root = configBuilder.Build();
EventGridChannelConfig config = new();
root.Bind(config);

IHostBuilder builder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddLogging();
        services.UseWebPipeline();
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

IPipelineService myservice = app.Services.GetRequiredService<IPipelineService>();
myservice.OnReceive += (s, e) =>
{
    string base64 = Encoding.UTF8.GetString(e.Message.Data.ToArray()).Trim('"');
    string text = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(text);
    Console.ResetColor();
};

//create a request
HttpRequestMessage request = new(HttpMethod.Post, "https://example.org");
request.Content = new StringContent("hi!");
request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

await myservice.ExecuteAsync(request);


