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
using System.Net.Http;

namespace Microsoft.Health.Fhir.Proxy.Configuration
{
    /// <summary>
    /// Helper extensions for pipelines
    /// </summary>
    public static class PipelineExtensions
    {
        /// <summary>
        /// Uses application insights for logging.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="instrumentationKey"></param>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public static IServiceCollection UseAppInsightsLogging(this IServiceCollection services, string instrumentationKey, LogLevel logLevel)
        {
            services.AddLogging(builder =>
            {
                builder.AddFilter<ApplicationInsightsLoggerProvider>("", logLevel);
                builder.AddApplicationInsights(instrumentationKey);
            });

            return services;
        }

        /// <summary>
        /// Use application insights for telemetry.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="instrumentationKey"></param>
        /// <returns></returns>
        public static IServiceCollection UseTelemetry(this IServiceCollection services, string instrumentationKey)
        {

            services.Configure<TelemetryConfiguration>(options =>
            {
                options.InstrumentationKey = instrumentationKey;
            });
            services.AddScoped<TelemetryClient>();

            return services;
            
        }

        /// <summary>
        /// Use the authenticator for acquisition of access tokens from Azure AD.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection UseAuthenticator(this IServiceCollection services, Action<ServiceIdentityOptions> options)
        {
            services.AddScoped<IAuthenticator, Authenticator>();
            services.Configure(options);

            return services;
        }

        /// <summary>
        /// Uses a Azure Function pipeline.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Use a Web pipeline for Web services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection UseWebPipeline(this IServiceCollection services, Action<PipelineOptions> options)
        {
            services.AddScoped<IInputFilterCollection, InputFilterCollection>();
            services.AddScoped<IOutputFilterCollection, OutputFilterCollection>();
            services.AddScoped<IInputChannelCollection, InputChannelCollection>();
            services.AddScoped<IOutputChannelCollection, OutputChannelCollection>();
            services.AddScoped<IPipeline<HttpRequestMessage, HttpResponseMessage>, WebPipeline>();
            services.Configure(options);
            return services;
        }

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
