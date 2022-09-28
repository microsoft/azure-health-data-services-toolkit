using System;
using System.Net.Http;
using Microsoft.AzureHealth.DataServices.Bindings;
using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AzureHealth.DataServices.Security;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace Microsoft.AzureHealth.DataServices.Configuration
{
    /// <summary>
    /// Helper extensions for pipelines
    /// </summary>
    public static class PipelineExtensions
    {
        /// <summary>
        /// Uses application insights for logging.
        /// </summary>
        /// <param name="services">IServiceColllection.</param>
        /// <param name="instrumentationConnectionString">AppInsights instrumentation key connection string.</param>
        /// <param name="logLevel">Log level.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection UseAppInsightsLogging(this IServiceCollection services, string instrumentationConnectionString, LogLevel logLevel)
        {
            services.AddLogging(builder =>
            {
                builder.AddFilter<ApplicationInsightsLoggerProvider>("", logLevel);
                builder.AddApplicationInsights(op => op.ConnectionString = instrumentationConnectionString, op => op.FlushOnDispose = true);
            });

            return services;
        }

        /// <summary>
        /// Use application insights for telemetry.
        /// </summary>
        /// <param name="services">IServicesCollection</param>
        /// <param name="instrumentationConnectionString">AppInsights instrumentation key connection string.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection UseTelemetry(this IServiceCollection services, string instrumentationConnectionString)
        {
            services.Configure<TelemetryConfiguration>(options =>
            {
                options.ConnectionString = instrumentationConnectionString;
            });
            services.AddScoped<TelemetryClient>();

            return services;
        }

        /// <summary>
        /// Use the authenticator for acquisition of access tokens from Azure AD.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="options">Options for configuration.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection UseAuthenticator(this IServiceCollection services, Action<ServiceIdentityOptions> options)
        {
            services.AddScoped<IAuthenticator, Authenticator>();
            services.Configure<ServiceIdentityOptions>(options);

            return services;
        }

        /// <summary>
        /// Use the authenticator with DefaultCredentials for acquistion of access tokens from Azure AD.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection UseAuthenticator(this IServiceCollection services)
        {
            return services.UseAuthenticator(options => options.CredentialType = null);
        }

        /// <summary>
        /// Uses a Azure Function pipeline.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection UseAzureFunctionPipeline(this IServiceCollection services)
        {
            services.AddScoped<IInputFilterCollection, InputFilterCollection>();
            services.AddScoped<IOutputFilterCollection, OutputFilterCollection>();
            services.AddScoped<IInputChannelCollection, InputChannelCollection>();
            services.AddScoped<IOutputChannelCollection, OutputChannelCollection>();
            services.AddScoped<IPipeline<HttpRequestData, HttpResponseData>, AzureFunctionPipeline>();
            return services;
        }



        /// <summary>
        /// Use a Web pipeline for Web services.
        /// </summary>
        /// <param name="services">>Services collection.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection UseWebPipeline(this IServiceCollection services)
        {
            services.AddScoped<IInputFilterCollection, InputFilterCollection>();
            services.AddScoped<IOutputFilterCollection, OutputFilterCollection>();
            services.AddScoped<IInputChannelCollection, InputChannelCollection>();
            services.AddScoped<IOutputChannelCollection, OutputChannelCollection>();
            services.AddScoped<IPipeline<HttpRequestMessage, HttpResponseMessage>, WebPipeline>();
            return services;
        }


        /// <summary>
        /// Use custom http headers to when sending http requests.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection UseCustomHeaders(this IServiceCollection services)
        {
            services.AddScoped<IHttpCustomHeaderCollection, HttpCustomHeaderCollection>();
            return services;
        }


        /// <summary>
        /// Adds a custom header for sending http requests.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="name">Name of the http header to add.</param>
        /// <param name="value">Value of the http header.</param>
        /// <param name="headerType">Type of custom header.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection AddCustomHeader(this IServiceCollection services, string name, string value, CustomHeaderType headerType)
        {
            services.Add(new ServiceDescriptor(typeof(IHeaderNameValuePair), new HeaderNameValuePair(name, value, headerType)));
            return services;
        }


        /// <summary>
        /// Adds an input filter.
        /// </summary>
        /// <typeparam name="TOptions">Type of options for input filter.</typeparam>
        /// <param name="services">Services collection.</param>
        /// <param name="type">Type of input filter.</param>
        /// <param name="options">Options for input filter.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection AddInputFilter<TOptions>(this IServiceCollection services, Type type, Action<TOptions> options) where TOptions : class
        {
            services.Add(new ServiceDescriptor(typeof(IInputFilter), type, ServiceLifetime.Scoped));
            services.Configure<TOptions>(options);
            return services;
        }

        /// <summary>
        /// Adds an input filter.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="type">Type of input filter.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection AddInputFilter(this IServiceCollection services, Type type)
        {
            services.Add(new ServiceDescriptor(typeof(IInputFilter), type, ServiceLifetime.Scoped));
            return services;
        }


        /// <summary>
        /// Add an output filter.
        /// </summary>
        /// <typeparam name="TOptions">Type of options for output filter.</typeparam>
        /// <param name="services">Services collection.</param>
        /// <param name="type">Type of output filter.</param>
        /// <param name="options">Options for output filter.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection AddOutputFilter<TOptions>(this IServiceCollection services, Type type, Action<TOptions> options) where TOptions : class
        {
            services.Add(new ServiceDescriptor(typeof(IOutputFilter), type, ServiceLifetime.Scoped));
            services.Configure<TOptions>(options);
            return services;
        }

        /// <summary>
        /// Adds an output filter.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="type">Type of output filter.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection AddOutputFilter(this IServiceCollection services, Type type)
        {
            services.Add(new ServiceDescriptor(typeof(IOutputFilter), type, ServiceLifetime.Scoped));
            return services;
        }

        /// <summary>
        /// Adds an input channel.
        /// </summary>
        /// <typeparam name="TOptions">Type of options for input channel.</typeparam>
        /// <param name="services">Services collection.</param>
        /// <param name="type">Type of input channel.</param>
        /// <param name="options">Options for input channel.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection AddInputChannel<TOptions>(this IServiceCollection services, Type type, Action<TOptions> options) where TOptions : class
        {
            services.Add(new ServiceDescriptor(typeof(IInputChannel), type, ServiceLifetime.Scoped));
            services.Configure<TOptions>(options);
            return services;
        }

        /// <summary>
        /// Add an output channel.
        /// </summary>
        /// <typeparam name="TOptions">Type of options for output channel.</typeparam>
        /// <param name="services">Services collection.</param>
        /// <param name="type">Type of output channel.</param>
        /// <param name="options">Options for output channel.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection AddOutputChannel<TOptions>(this IServiceCollection services, Type type, Action<TOptions> options) where TOptions : class
        {
            services.Add(new ServiceDescriptor(typeof(IOutputChannel), type, ServiceLifetime.Scoped));
            services.Configure<TOptions>(options);
            return services;
        }

        /// <summary>
        /// Add a binding
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="type">Type of binding.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection AddBinding(this IServiceCollection services, Type type)
        {
            services.Add(new ServiceDescriptor(typeof(IBinding), type, ServiceLifetime.Scoped));
            return services;
        }

        /// <summary>
        /// Adds a binding.
        /// </summary>
        /// <typeparam name="TOptions">Type of options for binding.</typeparam>
        /// <param name="services">Services collection.</param>
        /// <param name="type">Type of binding.</param>
        /// <param name="options">Options for binding.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection AddBinding<TOptions>(this IServiceCollection services, Type type, Action<TOptions> options) where TOptions : class
        {
            services.Add(new ServiceDescriptor(typeof(IBinding), type, ServiceLifetime.Scoped));
            services.Configure<TOptions>(options);
            return services;
        }


        /// <summary>
        /// Adds a REST binding.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="options">Options for REST binding.</param>
        /// <returns>Services collection.</returns>
        public static IServiceCollection AddRestBinding(this IServiceCollection services, Action<RestBindingOptions> options)
        {
            services.Add(new ServiceDescriptor(typeof(IBinding), typeof(RestBinding), ServiceLifetime.Scoped));
            services.Configure<RestBindingOptions>(options);
            return services;
        }

    }
}
