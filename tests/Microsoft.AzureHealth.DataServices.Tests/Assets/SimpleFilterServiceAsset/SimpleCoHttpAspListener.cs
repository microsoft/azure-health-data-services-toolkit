using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets.SimpleFilterServiceAsset
{
    public class SimpleCoHttpAspListener
    {
        public static void Configure(IApplicationBuilder app)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("Simple", "{controller=Simple}/{id}");
            });
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.UseWebPipeline();
            services.AddInputFilter<SimpleFilterOptions>(typeof(SimpleFilter), options =>
            {
                options.BaseUrl = new Uri("http://localhost:1212");
                options.HttpMethod = HttpMethod.Post;
                options.Path = "echo";
                options.ExecutionStatus = DataServices.Pipelines.StatusType.Any;
            });
            services.UseCustomHeaders();
            services.AddCustomHeader("X-MS-Test", "customvalue", CustomHeaderType.RequestStatic);
            services.AddCustomHeader("X-MS-Identity", "name", CustomHeaderType.RequestIdentity);

            services.AddRouting();
            services.AddMvcCore();
        }
    }
}
