using System;
using System.Net.Http;
using Azure.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AzureHealth.DataServices.Bindings;
using Microsoft.AzureHealth.DataServices.Channels;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Microsoft.AzureHealth.DataServices.Security;
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
                builder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, logLevel);
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
        /// <returns>Modified Services collection.</returns>
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
        /// <returns>Modified Services collection.</returns>
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
        /// <returns>Modified Services collection.</returns>
        public static IServiceCollection AddInputFilter<TOptions>(this IServiceCollection services, Type type, Action<TOptions> options)
            where TOptions : class
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
        /// <returns>Modified Services collection.</returns>
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
        /// <returns>Modified Services collection.</returns>
        public static IServiceCollection AddOutputFilter<TOptions>(this IServiceCollection services, Type type, Action<TOptions> options)
            where TOptions : class
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
        /// <returns>Modified Services collection.</returns>
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
        /// <returns>Modified Services collection.</returns>
        public static IServiceCollection AddInputChannel<TOptions>(this IServiceCollection services, Type type, Action<TOptions> options)
            where TOptions : class
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
        /// <returns>Modified Services collection.</returns>
        public static IServiceCollection AddOutputChannel<TOptions>(this IServiceCollection services, Type type, Action<TOptions> options)
            where TOptions : class
        {
            services.Add(new ServiceDescriptor(typeof(IOutputChannel), type, ServiceLifetime.Scoped));
            services.Configure<TOptions>(options);
            return services;
        }

        /// <summary>
        /// Adds an instance of the specified binding type <typeparamref name="TBinding"/> to the <paramref name="services"/> collection, and configures an <see cref="HttpClient"/> instance for the binding with the specified <paramref name="baseAddress"/>.
        /// </summary>
        /// <typeparam name="TBinding">The binding type to add to the <paramref name="services"/> collection.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to add the binding to.</param>
        /// <param name="baseAddress">The base URI address for the binding's client.</param>
        /// <param name="credential">Credential used by the binding to access the target.</param>
        /// <param name="scopes">Scopes used by the binding to access the target.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> instance for the binding's client.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="baseAddress"/> is null.</exception>
        public static IHttpClientBuilder AddBinding<TBinding>(this IServiceCollection services, Uri baseAddress, TokenCredential credential = null, string[] scopes = default)
            where TBinding : class, IBinding
        {
            services.Add(new ServiceDescriptor(typeof(IBinding), typeof(TBinding), ServiceLifetime.Scoped));

            IHttpClientBuilder clientBuilder = services.AddHttpClient<IBinding, TBinding>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            if (credential is not null)
            {
                clientBuilder.UseCredential(credential, baseAddress, scopes);
            }

            return clientBuilder;
        }

        /// <summary>
        /// Adds an instance of the specified binding type <typeparamref name="TBinding"/> to the <paramref name="services"/> collection, configures it with the specified <paramref name="options"/>, and configures an <see cref="HttpClient"/> instance using the configuration object/>.
        /// </summary>
        /// <typeparam name="TBinding">The binding type to add to the <paramref name="services"/> collection.</typeparam>
        /// <typeparam name="TOptions">The type of options to configure for the binding. Must implement the <see cref="IBinding"/> interface.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to add the binding to.</param>
        /// <param name="options">An action that configures the <typeparamref name="TOptions"/> instance for the binding.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> instance for the binding's client.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="options"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an instance of <typeparamref name="TOptions"/> cannot be created using the default constructor.</exception>
        public static IHttpClientBuilder AddBinding<TBinding, TOptions>(this IServiceCollection services, Action<TOptions> options)
            where TBinding : class, IBinding
            where TOptions : class, IBindingOptions
        {
            TOptions optionsValue = Activator.CreateInstance<TOptions>();
            options(optionsValue);

            services.Configure<TOptions>(options);
            return services.AddBinding<TBinding>(optionsValue.BaseAddress, optionsValue.Credential, optionsValue.Scopes);
        }

        /// <summary>
        /// Configures the <see cref="IHttpClientBuilder"/> to use the specified <paramref name="credential"/> for authentication using the Azure Token Credential library.
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/> instance to configure.</param>
        /// <param name="credential">The <see cref="TokenCredential"/> instance to use for authentication.</param>
        /// <param name="baseAddress">Base address for the client using the credential. Used for resource based scoping via {{baseAddress}}/.default</param>
        /// <param name="scopes">The optional list of scopes required for the authentication. If omitted, the default value of null is used.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> instance with the <see cref="BearerTokenHandler"/> added to its message handlers.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="credential"/> is null.</exception>
        public static IHttpClientBuilder UseCredential(this IHttpClientBuilder builder, TokenCredential credential, Uri baseAddress, string[] scopes = default)
        {
            return builder.AddHttpMessageHandler(x => new BearerTokenHandler(credential, baseAddress, scopes));
        }
    }
}
