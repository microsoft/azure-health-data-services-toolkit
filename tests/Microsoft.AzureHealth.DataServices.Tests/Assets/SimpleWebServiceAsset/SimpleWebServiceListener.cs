using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets.SimpleWebServiceAsset
{
    public class SimpleWebServiceListener
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
                endpoints.MapControllerRoute("Echo", "{controller=Echo}/{id}");
            });
        }

        public static void ConfigureServices(IServiceCollection services)
        {

            services.AddRouting();
            services.AddMvcCore();
        }
    }
}
