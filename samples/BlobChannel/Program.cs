
using BlobChannelSample;
using DataServices.Channels;
using DataServices.Configuration;
using DataServices.Pipelines;
using DataServices.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

IConfigurationBuilder configBuilder = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
    .AddEnvironmentVariables("PROXY_");
IConfigurationRoot root = configBuilder.Build();
BlobChannelConfig config = new();
root.Bind(config);



IHostBuilder builder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddLogging();
        services.UseWebPipeline();

        services.AddInputChannel<BlobStorageChannelOptions>(typeof(BlobStorageChannel), options =>
        {
            options.ConnectionString = config.ConnectionString;
            options.Container = config.Container;
            options.ExecutionStatusType = StatusType.Normal;
        });

        services.AddSingleton<IPipelineService, PipelineService>();
    });

var app = builder.Build();
app.RunAsync().GetAwaiter();

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

await service.ExecuteAsync(request);

StorageBlob storage = new(config.ConnectionString);
var list = await storage.ListBlobsAsync(config.Container);
Console.ForegroundColor = ConsoleColor.Yellow;
foreach(var item in list)
{
    byte[] blob = await storage.ReadBlockBlobAsync(config.Container, item);
    string payloadJson = Encoding.UTF8.GetString(blob);
    Payload output = JsonConvert.DeserializeObject<Payload>(payloadJson);
    Console.WriteLine($"Name={payload.Name}  Value={payload.Value}");
    await storage.DeleteBlobAsync(config.Container, item);
}
Console.ResetColor();


