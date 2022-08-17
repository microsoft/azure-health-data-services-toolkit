
using Azure.Health.DataServices.Channels;
using Azure.Health.DataServices.Configuration;
using Azure.Health.DataServices.Pipelines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using ServiceBusChannelSample;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

IConfigurationBuilder configBuilder = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
    .AddEnvironmentVariables("AZURE_");
IConfigurationRoot root = configBuilder.Build();
MyServiceConfig config = new();
root.Bind(config);


IHostBuilder builder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddLogging();
        services.UseWebPipeline();
        services.AddInputChannel<ServiceBusChannelOptions>(typeof(ServiceBusChannel), options =>
        {
            options.ConnectionString = config.ConnectionString;
            options.FallbackStorageConnectionString = config.FallbackStorageConnectionString;
            options.FallbackStorageContainer = config.FallbackStorageContainer;
            options.ExecutionStatusType = StatusType.Normal;
            options.Sku = config.Sku;
            options.Topic = config.Topic;
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
    string json = Encoding.UTF8.GetString(e.Message);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Message Received !!!");
    Payload payload = JsonConvert.DeserializeObject<Payload>(json);
    Console.WriteLine($"Content Received Name = {payload.Name}  Value = {payload.Value}");
    Console.ResetColor();
};

Payload payload = new() { Name = "payloadName", Value = "payloadValue" };
string json = JsonConvert.SerializeObject(payload);
HttpRequestMessage message = new(HttpMethod.Post, "http://example.org/");
message.Content = new StringContent(json);
message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
message.Content.Headers.ContentLength = json.Length;

Console.WriteLine("Message sent to Service Bus...");
await myservice.SendAsync(message);
Console.WriteLine("waiting to receive message from Service Bus...");
await Task.Delay(10000);
