using Microsoft.Extensions.DependencyInjection;

namespace AuthenticatorSample
{
    public static class Extensions
    {
        // This adds a nice, builder pattern for Program.cs. This is not necessary, but helpful
        // if you have multiple services in your application.
        public static IServiceCollection UseMyService(this IServiceCollection services, Action<MyServiceOptions> options)
        {
            services.AddScoped<IMyService, MyService>();
            services.Configure(options);
            return services;
        }
    }
}
