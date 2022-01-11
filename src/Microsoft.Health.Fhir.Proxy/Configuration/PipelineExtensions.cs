using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Health.Fhir.Proxy.Bindings;
using Microsoft.Health.Fhir.Proxy.Channels;
using Microsoft.Health.Fhir.Proxy.Filters;
using Microsoft.Health.Fhir.Proxy.Pipelines;
using Microsoft.Health.Fhir.Proxy.Security;
using System;

namespace Microsoft.Health.Fhir.Proxy.Configuration
{
    public static class PipelineExtensions
    {
        public static IServiceCollection UseAppInsightsLogging(this IServiceCollection services, string instrumentationKey, LogLevel logLevel)
        {
            services.AddLogging(builder =>
            {
                builder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace);
                builder.AddApplicationInsights(instrumentationKey);
            });

            return services;
        }

        public static IServiceCollection UseTelemetry(this IServiceCollection services, string appInsightsConnectionString)
        {

            services.Configure<TelemetryConfiguration>(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            });
            services.AddScoped<TelemetryClient>();

            return services;
            
        }

        public static IServiceCollection UseAuthenticator(this IServiceCollection services, Action<ServiceIdentityOptions> options)
        {
            services.AddScoped<IAuthenticator, Authenticator>();
            services.Configure(options);

            return services;
        }

        public static IServiceCollection UseAzureFunctionPipeline(this IServiceCollection services, Action<PipelineOptions> options)
        {
            services.AddScoped<IInputFilterCollection, InputFilterCollection>();
            services.AddScoped<IOutputFilterCollection, OutputFilterCollection>();
            services.AddScoped<IInputChannelCollection, InputChannelCollection>();
            services.AddScoped<IOutputChannelCollection, OutputChannelCollection>();
            services.AddScoped<IPipeline<HttpRequestData, HttpResponseData>, AzureFunctionPipeline>();
            services.Configure(options);
            return services;
        }

        public static IServiceCollection UseWebPipeline(this IServiceCollection services, Action<PipelineOptions> options)
        {
            services.AddScoped<IInputFilterCollection, InputFilterCollection>();
            services.AddScoped<IOutputFilterCollection, OutputFilterCollection>();
            services.AddScoped<IInputChannelCollection, InputChannelCollection>();
            services.AddScoped<IOutputChannelCollection, OutputChannelCollection>();
            services.AddScoped(typeof(WebPipeline));
            //services.ConfigureOptions(options);
            services.Configure(options);
            return services;
        }

        //public static IServiceCollection AddInputFilter<TFilter, TOptions>(
        //    this IServiceCollection services,
        //    Action<TOptions> options) where TFilter : class, IFilter, new()
        //                              where TOptions : class
        //{
        //    services.AddScoped<IFilter, TFilter>();
        //    services.Configure(options);
        //    return services;
        //}


        public static IServiceCollection AddInputFilter<TOptions>(this IServiceCollection services, Type type, Action<TOptions> options) where TOptions : class
        {
            services.Add(new ServiceDescriptor(typeof(IInputFilter), type, ServiceLifetime.Scoped));
            services.Configure(options);
            return services;
        }


        public static IServiceCollection AddOutputFilter<TOptions>(this IServiceCollection services, Type type, Action<TOptions> options) where TOptions : class
        {
            services.Add(new ServiceDescriptor(typeof(IOutputFilter), type, ServiceLifetime.Scoped));
            services.Configure(options);
            return services;
        }

        public static IServiceCollection AddInputChannel<TOptions>(this IServiceCollection services, Type type, Action<TOptions> options) where TOptions : class
        {
            services.Add(new ServiceDescriptor(typeof(IInputChannel), type, ServiceLifetime.Scoped));
            services.Configure(options);
            return services;
        }

        public static IServiceCollection AddOutputChannel<TOptions>(this IServiceCollection services, Type type, Action<TOptions> options) where TOptions : class
        {
            services.Add(new ServiceDescriptor(typeof(IOutputChannel), type, ServiceLifetime.Scoped));
            services.Configure(options);
            return services;
        }

        public static IServiceCollection AddBinding(this IServiceCollection services, Type type)
        {
            services.Add(new ServiceDescriptor(typeof(IBinding), type, ServiceLifetime.Scoped));
            return services;
        }

    }
}
