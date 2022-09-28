// See https://aka.ms/new-console-template for information on 
using BlobChannelSample;
using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Configuration;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AzureHealth.DataServices.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
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
BlobChannelConfig config = new();
root.Bind(config);

// Configures the custom operation.
IHostBuilder builder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddLogging();

        // This creates the custom operation pipeline.
        services.UseWebPipeline();

        // This adds the Blob Storage channel at the input stage in the pipeline.
        // This means the Blob Storage channel is called after the input filters but before any bindings.
        services.AddInputChannel<BlobStorageChannelOptions>(typeof(BlobStorageChannel), options =>
        {
            options.ConnectionString = config.ConnectionString;
            options.Container = config.Container;
            options.ExecutionStatusType = StatusType.Normal;
        });

        // This adds a sample service that can be called in our pipeline
        services.AddSingleton<IPipelineService, PipelineService>();
    });

// Set up the pipeline
var app = builder.Build();
app.RunAsync().GetAwaiter();

// Tests the sample
await TestSample();

async Task TestSample()
{
    // Setup a sample request for the pipeline
    string name = RandomString.GetRandomString(6);
    string value = RandomString.GetRandomString(6);

    Payload payload = new() { Name = name, Value = value };
    string json = JsonConvert.SerializeObject(payload);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Sending Name={name} Value={value}");
    Console.ResetColor();

    HttpRequestMessage request = new(HttpMethod.Post, "https://example.org");
    request.Content = new StringContent(json);
    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
    request.Content.Headers.ContentLength = json.Length;

    IPipelineService service = app.Services.GetRequiredService<IPipelineService>();

    // Sends an example request in the pipeline
    await service.ExecuteAsync(request);

    // Show what's the the blob container after the sample
    StorageBlob storage = new(config.ConnectionString);
    var list = await storage.ListBlobsAsync(config.Container);
    Console.ForegroundColor = ConsoleColor.Yellow;
    foreach (var item in list)
    {
        byte[] blob = await storage.ReadBlockBlobAsync(config.Container, item);
        string payloadJson = Encoding.UTF8.GetString(blob);
        Payload output = JsonConvert.DeserializeObject<Payload>(payloadJson);
        Console.WriteLine($"Name={payload.Name}  Value={payload.Value}");
        await storage.DeleteBlobAsync(config.Container, item);
    }
    Console.ResetColor();
}

