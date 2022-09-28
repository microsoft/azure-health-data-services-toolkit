using Microsoft.AzureHealth.DataServices.Bindings;
using Microsoft.AzureHealth.DataServices.Configuration;
using Microsoft.AzureHealth.DataServices.Pipelines;
using SimpleCustomOperation.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.UseWebPipeline();
builder.Services.AddInputFilter<SimpleInputFilterOptions>(typeof(SimpleInputFilter), options =>
{
    options.BaseUrl = "http://localhost:7777";
    options.HttpMethod = "POST";
    options.Path = "simple";
    options.ExecutionStatus = StatusType.Any;
});

builder.Services.AddBinding<RestBindingOptions>(typeof(RestBinding), options =>
{
    options.ServerUrl = "http://localhost:7777";
});

builder.Services.AddOutputFilter<SimpleOutputFilterOptions>(typeof(SimpleOutputFilter), options =>
{
    options.ExecutionStatus = StatusType.Normal;
});


builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run("http://localhost:7776");
