using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Configuration;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using ServiceBusChannelSample;

#pragma warning disable CA1852
internal static class Program
{
    private static async Task Main(string[] args)
    {
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

                // Add Service Bus the first input channel. Azure Storage is a backing store for events that are
                // too large for the Service Bus SKU.
                services.AddInputChannel<ServiceBusChannelOptions>(typeof(ServiceBusChannel), options =>
                {
                    options.ConnectionString = config.ConnectionString;
                    options.FallbackStorageConnectionString = config.FallbackStorageConnectionString;
                    options.FallbackStorageContainer = config.FallbackStorageContainer;
                    options.ExecutionStatusType = StatusType.Normal;
                    options.Sku = config.Sku;
                    options.Topic = config.Topic;
                });

                services.AddSingleton(config);
                services.AddSingleton<IMyService, MyService>();
            });

        IHost app = builder.Build();
        app.RunAsync().GetAwaiter();

        // Get our wrapper for the pipeline service. This service allows us to use our pipeline for sending/receiving data to/from Service Bus in a pipeline.
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

        // Create a test message to send
        Payload payload = new() { Name = "payloadName", Value = "payloadValue" };
        string json = JsonConvert.SerializeObject(payload);
        HttpRequestMessage message = new(HttpMethod.Post, "http://example.org/");
        message.Content = new StringContent(json);
        message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        message.Content.Headers.ContentLength = json.Length;

        // Send test message
        Console.WriteLine("Message sent to Service Bus...");
        await myservice.SendAsync(message);
        Console.WriteLine("waiting to receive message from Service Bus...");
        await Task.Delay(10000);
        message.Dispose();
    }
}
