using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using EnsureThat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureHealth.DataServices.Clients
{
    /// <summary>
    /// 
    /// </summary>
    public class FhirClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FhirClient"/> class for mocking.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        protected FhirClient()
        {
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// Initializes a new instance of the <see cref="FhirClient"/> class.
        /// </summary>
        public FhirClient(Uri fhirUri, TokenCredential credential)
            : this(fhirUri, credential, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FhirClient"/> class.
        /// </summary>
        public FhirClient(Uri fhirUri, TokenCredential credential, FhirClientOptions? options)
        {
            if (fhirUri is null)
                throw new ArgumentNullException(nameof(fhirUri));
            if (credential is null)
                throw new ArgumentNullException(nameof(credential));

            _fhirUri = fhirUri;
            options ??= new FhirClientOptions();

            _pipeline = CreatePipeline(fhirUri, options, credential);
        }

        private readonly Uri _fhirUri;
        private readonly HttpPipeline _pipeline;

        /// <summary> The HTTP pipeline for sending and receiving REST requests and responses. </summary>
        public virtual HttpPipeline Pipeline => _pipeline;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="resourceId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="RequestFailedException"></exception>
        public virtual async Task<Response<JObject>> GetAsync(string resourceType, string resourceId, CancellationToken cancellationToken = default)
        {
            using HttpMessage message = CreateGetRequest(resourceType, resourceId);
            await _pipeline.SendAsync(message, cancellationToken).ConfigureAwait(false);

            switch (message.Response.Status)
            {
                case 200:
                    {
                        using StreamReader stream = new(message.Response.ContentStream!);
                        using JsonTextReader reader = new(stream);
                        JObject responseObject = (JObject)JToken.ReadFrom(reader);
                        return Response.FromValue(responseObject, message.Response);
                    }
                default:
                    throw new RequestFailedException(message.Response.Status, "Invalid status code when executing get resource.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="RequestFailedException"></exception>
        public virtual async Task<Response<JObject>> SearchAsync(string resourceType, NameValueCollection parameters, CancellationToken cancellationToken = default)
        {
            using HttpMessage message = CreateSearchRequest(resourceType, parameters);
            await _pipeline.SendAsync(message, cancellationToken).ConfigureAwait(false);

            switch (message.Response.Status)
            {
                case 200:
                    {
                        using StreamReader stream = new(message.Response.ContentStream!);
                        using JsonTextReader reader = new(stream);
                        JObject responseObject = (JObject)JToken.ReadFrom(reader);
                        return Response.FromValue(responseObject, message.Response);
                    }
                default:
                    throw new RequestFailedException(message.Response.Status, "Invalid status code when executing search.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="RequestFailedException"></exception>
        public virtual async Task<Response<JObject>> GetExportJobAsync(string jobId, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrWhiteSpace(jobId, nameof(jobId));

            using HttpMessage message = CreateGetExportJobRequest(jobId);
            await _pipeline.SendAsync(message, cancellationToken).ConfigureAwait(false);

            switch (message.Response.Status)
            {
                case 200:
                case 202:
                    {
                        using StreamReader stream = new(message.Response.ContentStream!);
                        using JsonTextReader reader = new(stream);
                        JObject responseObject = (JObject)JToken.ReadFrom(reader);
                        return Response.FromValue(responseObject, message.Response);
                    }
                default:
                    throw new RequestFailedException(message.Response.Status, "Invalid status code when fetching export results.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeFilter"></param>
        /// <param name="container"></param>
        /// <param name="since"></param>
        /// <param name="till"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="RequestFailedException"></exception>
        public virtual async Task<Response<string>> SystemExportAsync(string? type, string? typeFilter, string? container, DateTime? since, DateTime? till, CancellationToken cancellationToken = default)
        {
            using HttpMessage message = CreateSystemExportRequest(type, type, container, since, till);
            await _pipeline.SendAsync(message, cancellationToken).ConfigureAwait(false);
            switch (message.Response.Status)
            {
                case 200:
                case 202:
                    {
                        // #TODO = checking this value.
                        var exportJobUri = new Uri(message.Response.Headers.Single(x => x.Name == "Content-Location").Value);
                        return Response.FromValue(exportJobUri.Segments.Last(), message.Response);
                    }
                default:
                    throw new RequestFailedException(message.Response.Status, "Invalid status code when running system export operation.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeFilter"></param>
        /// <param name="container"></param>
        /// <param name="since"></param>
        /// <param name="till"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="RequestFailedException"></exception>
        public virtual async Task<Response<string>> PatientExportAsync(string? type, string? typeFilter, string? container, DateTime? since, DateTime? till, CancellationToken cancellationToken = default)
        {
            using HttpMessage message = CreatePatientExportRequest(type, type, container, since, till);
            await _pipeline.SendAsync(message, cancellationToken).ConfigureAwait(false);
            switch (message.Response.Status)
            {
                case 200:
                case 202:
                    {
                        // #TODO = checking this value.
                        var exportJobUri = new Uri(message.Response.Headers.Single(x => x.Name == "Content-Location").Value);
                        return Response.FromValue(exportJobUri.Segments.Last(), message.Response);
                    }
                default:
                    throw new RequestFailedException(message.Response.Status, "Invalid status code when running patient export operation.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="type"></param>
        /// <param name="typeFilter"></param>
        /// <param name="container"></param>
        /// <param name="since"></param>
        /// <param name="till"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="RequestFailedException"></exception>
        public virtual async Task<Response<string>> GroupExportAsync(string groupId, string? type, string? typeFilter, string? container, DateTime? since, DateTime? till, CancellationToken cancellationToken = default)
        {
            if (groupId is null)
                throw new ArgumentNullException(nameof(groupId));

            using HttpMessage message = CreateGroupExportRequest(groupId, type, typeFilter, container, since, till);
            await _pipeline.SendAsync(message, cancellationToken).ConfigureAwait(false);

            switch (message.Response.Status)
            {
                case 200:
                case 202:
                    {
                        // #TODO = checking this value.
                        var exportJobUri = new Uri(message.Response.Headers.Single(x => x.Name == "Content-Location").Value);
                        return Response.FromValue(exportJobUri.Segments.Last(), message.Response);
                    }
                default:
                    throw new RequestFailedException(message.Response.Status, "Invalid status code when running group export operation.");
            }
        }

        private static HttpPipeline CreatePipeline(Uri endpoint, FhirClientOptions options, TokenCredential credential)
        {
            return HttpPipelineBuilder.Build(options,
                Array.Empty<HttpPipelinePolicy>(),
                new HttpPipelinePolicy[] {
                    new BearerTokenAuthenticationPolicy(credential, options.Scope ?? GetDefaultScope(endpoint))
                },
                new ResponseClassifier()
            );
        }

        private HttpMessage CreateGetRequest(string resourceType, string resourceId)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = RequestMethod.Get;

            var uri = new RequestUriBuilder();
            uri.Reset(_fhirUri);
            uri.AppendPath("/", false);
            uri.AppendPath(resourceType, true);
            uri.AppendPath("/", false);
            uri.AppendPath(resourceId, true);

            request.Uri = uri;

            request.Headers.Add("Accept", "application/fhir+json");
            return message;
        }

        private HttpMessage CreateSearchRequest(string resourceType, NameValueCollection parameters)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = RequestMethod.Get;

            var uri = new RequestUriBuilder();
            uri.Reset(_fhirUri);
            uri.AppendPath("/", false);
            uri.AppendPath(resourceType, true);

            foreach (var parameterName in parameters.AllKeys)
            {
                uri.AppendQuery(parameterName!, parameters[parameterName]!);
            }

            request.Uri = uri;

            request.Headers.Add("Accept", "application/fhir+json");
            return message;
        }

        private HttpMessage CreateSystemExportRequest(string? type, string? typeFilter, string? container, DateTime? since, DateTime? till)
        {
            return CreateExportRequest(string.Empty, type, typeFilter, container, since, till);
        }

        private HttpMessage CreatePatientExportRequest(string? type, string? typeFilter, string? container, DateTime? since, DateTime? till)
        {
            return CreateExportRequest("/Patient", type, typeFilter, container, since, till);
        }

        private HttpMessage CreateGroupExportRequest(string groupId, string? type, string? typeFilter, string? container, DateTime? since, DateTime? till)
        {
            var uri = new RequestUriBuilder();
            uri.Reset(_fhirUri);
            uri.AppendPath("/Group/", false);
            uri.AppendPath(groupId, true);
            uri.AppendPath("/$export", false);

            return CreateExportRequest(uri.Path, type, typeFilter, container, since, till);
        }

        private HttpMessage CreateExportRequest(string exportPath, string? type, string? typeFilter, string? container, DateTime? since, DateTime? till)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = RequestMethod.Get;

            var uri = new RequestUriBuilder();
            uri.Reset(_fhirUri);
            uri.AppendPath(exportPath, false);

            if (type != null)
            {
                uri.AppendQuery("_type", type);
            }
            if (type != null && typeFilter != null)
            {
                uri.AppendQuery("_typeFilter", typeFilter);
            }
            if (container != null)
            {
                uri.AppendQuery("_container", container);
            }
            if (since != null)
            {
                uri.AppendQuery("_since", since.Value.ToString());
            }
            if (till != null)
            {
                uri.AppendQuery("_till", till.Value.ToString());
            }

            request.Uri = uri;

            request.Headers.Add("Accept", "application/fhir+json");
            request.Headers.Add("Prefer", "respond-async");
            return message;
        }

        private HttpMessage CreateGetExportJobRequest(string jobId)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = RequestMethod.Get;

            var uri = new RequestUriBuilder();
            uri.Reset(_fhirUri);
            uri.AppendPath("/_operations/export/", false);
            uri.AppendPath(jobId, true);
            request.Uri = uri;

            return message;
        }

        private static string GetDefaultScope(Uri uri)
            => $"{uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped)}/.default";
    }
}
