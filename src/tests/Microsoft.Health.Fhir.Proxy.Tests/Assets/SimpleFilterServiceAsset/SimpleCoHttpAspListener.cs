using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Fhir.Proxy.Configuration;

namespace Microsoft.Health.Fhir.Proxy.Tests.Assets.SimpleFilterServiceAsset
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
                options.BaseUrl = $"http://localhost:1212";
                options.HttpMethod = "Post";
                options.Path = "echo";
                options.ExecutionStatus = Microsoft.Health.Fhir.Proxy.Pipelines.StatusType.Any;
            });
            services.UseCustomHeaders();
            services.AddCustomHeader(options =>
            {
                options.Name = "X-MS-Test";
                options.Value = "customvalue";
            });
            services.UseCustomIdentityHeaders();
            services.AddCustomIdentityHeader(options =>
            {
                options.HeaderName = "X-MS-Identity";
                options.ClaimType = "name";
            });

            services.AddRouting();
            services.AddMvcCore();
        }
    }
}
