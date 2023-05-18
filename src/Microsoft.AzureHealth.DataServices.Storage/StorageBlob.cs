using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureHealth.DataServices.Storage
{
    /// <summary>
    /// Allows simple access to Azure Blob Storage.
    /// </summary>
    public class StorageBlob
    {
        private readonly MemoryCacheEntryOptions _cacheOptions = new();
        private readonly MemoryCache containerCache;
        private readonly ILogger logger;
        private readonly BlobServiceClient blobService;
        private BlobContainerClient containerClient;
        private StorageTransferOptions storageTransferOptions;

#pragma warning disable SA1124 // Do not use regions

        #region ctor

        /// <summary>
        /// Creates an instance of StorageBlob
        /// </summary>
        /// <param name="connectionString">Azure storage connection string.</param>
        /// <param name="initialTransferSize">Optional size of the first range request in bytes. Blobs smaller than this limit will be downloaded in a single request. Blobs larger than this limit will continue being downloaded in chunks of size maxTransfersize.</param>
        /// <param name="maxConcurrency">Optional maximum number of workers that may be used in a parallel transfer.</param>
        /// <param name="maxTransferSize">Optional The maximum length of an transfer in bytes.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageBlob(string connectionString, long? initialTransferSize = null, int? maxConcurrency = null, int? maxTransferSize = null, ILogger logger = null)
            : this(initialTransferSize, maxConcurrency, maxTransferSize, logger)
        {
            blobService = new BlobServiceClient(connectionString);
        }

        /// <summary>
        /// Creates an instance of StorageBlob.
        /// </summary>
        /// <param name="connectionString">Azure storage connection string.</param>
        /// <param name="options">Blob client options.</param>
        /// <param name="initialTransferSize">Optional size of the first range request in bytes. Blobs smaller than this limit will be downloaded in a single request. Blobs larger than this limit will continue being downloaded in chunks of size maxTransfersize.</param>
        /// <param name="maxConcurrency">Optional maximum number of workers that may be used in a parallel transfer.</param>
        /// <param name="maxTransferSize">Optional The maximum length of an transfer in bytes.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageBlob(string connectionString, BlobClientOptions options, long? initialTransferSize = null, int? maxConcurrency = null, int? maxTransferSize = null, ILogger logger = null)
            : this(initialTransferSize, maxConcurrency, maxTransferSize, logger)
        {
            blobService = new BlobServiceClient(connectionString, options);
        }

        /// <summary>
        /// Creates an instance of StorageBlob.
        /// </summary>
        /// <param name="serviceUri">Service URI.</param>
        /// <param name="options">Optional blob client options.</param>
        /// <param name="initialTransferSize">Optional size of the first range request in bytes. Blobs smaller than this limit will be downloaded in a single request. Blobs larger than this limit will continue being downloaded in chunks of size maxTransfersize.</param>
        /// <param name="maxConcurrency">Optional maximum number of workers that may be used in a parallel transfer.</param>
        /// <param name="maxTransferSize">Optional The maximum length of an transfer in bytes.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageBlob(Uri serviceUri, BlobClientOptions options = null, long? initialTransferSize = null, int? maxConcurrency = null, int? maxTransferSize = null, ILogger logger = null)
            : this(initialTransferSize, maxConcurrency, maxTransferSize, logger)
        {
            blobService = new BlobServiceClient(serviceUri, options);
        }

        /// <summary>
        /// Creates an instance of StorageBlob.
        /// </summary>
        /// <param name="serviceUri">Service URI.</param>
        /// <param name="credentials">Token credentials for authentication.</param>
        /// <param name="options">Optional Blob client options.</param>
        /// <param name="initialTransferSize">Optional size of the first range request in bytes. Blobs smaller than this limit will be downloaded in a single request. Blobs larger than this limit will continue being downloaded in chunks of size maxTransfersize.</param>
        /// <param name="maxConcurrency">Optional maximum number of workers that may be used in a parallel transfer.</param>
        /// <param name="maxTransferSize">Optional The maximum length of an transfer in bytes.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageBlob(Uri serviceUri, TokenCredential credentials, BlobClientOptions options = null, long? initialTransferSize = null, int? maxConcurrency = null, int? maxTransferSize = null, ILogger logger = null)
            : this(initialTransferSize, maxConcurrency, maxTransferSize, logger)
        {
            blobService = new BlobServiceClient(serviceUri, credentials, options);
        }

        /// <summary>
        /// Creates an instance of StorageBlob.
        /// </summary>
        /// <param name="serviceUri">Service URI</param>
        /// <param name="credential">SAS credentials for authentication.</param>
        /// <param name="options">Optional blob client options.</param>
        /// <param name="initialTransferSize">Optional size of the first range request in bytes. Blobs smaller than this limit will be downloaded in a single request. Blobs larger than this limit will continue being downloaded in chunks of size maxTransfersize.</param>
        /// <param name="maxConcurrency">Optional maximum number of workers that may be used in a parallel transfer.</param>
        /// <param name="maxTransferSize">Optional The maximum length of an transfer in bytes.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageBlob(Uri serviceUri, AzureSasCredential credential, BlobClientOptions options = null, long? initialTransferSize = null, int? maxConcurrency = null, int? maxTransferSize = null, ILogger logger = null)
             : this(initialTransferSize, maxConcurrency, maxTransferSize, logger)
        {
            blobService = new BlobServiceClient(serviceUri, credential, options);
        }

        /// <summary>
        /// Creates an instance of StorageBlob.
        /// </summary>
        /// <param name="serviceUri">Service URI.</param>
        /// <param name="credential">Shared key credential for authentication.</param>
        /// <param name="options">Optional blob client options.</param>
        /// <param name="initialTransferSize">Optional size of the first range request in bytes. Blobs smaller than this limit will be downloaded in a single request. Blobs larger than this limit will continue being downloaded in chunks of size maxTransfersize.</param>
        /// <param name="maxConcurrency">Optional maximum number of workers that may be used in a parallel transfer.</param>
        /// <param name="maxTransferSize">Optional The maximum length of an transfer in bytes.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageBlob(Uri serviceUri, StorageSharedKeyCredential credential, BlobClientOptions options = null, long? initialTransferSize = null, int? maxConcurrency = null, int? maxTransferSize = null, ILogger logger = null)
             : this(initialTransferSize, maxConcurrency, maxTransferSize, logger)
        {
            blobService = new BlobServiceClient(serviceUri, credential, options);
        }

        /// <summary>
        /// Creates an instance of StorageBlob.
        /// </summary>
        /// <param name="initialTransferSize">Optional size of the first range request in bytes. Blobs smaller than this limit will be downloaded in a single request. Blobs larger than this limit will continue being downloaded in chunks of size maxTransfersize.</param>
        /// <param name="maxConcurrency">Optional maximum number of workers that may be used in a parallel transfer.</param>
        /// <param name="maxTransferSize">Optional The maximum length of an transfer in bytes.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageBlob(long? initialTransferSize, int? maxConcurrency, int? maxTransferSize, ILogger logger = null)
        {
            _cacheOptions = new MemoryCacheEntryOptions();
            _cacheOptions.SetSlidingExpiration(TimeSpan.FromSeconds(300));
            MemoryCacheOptions memOptions = new()
            {
                ExpirationScanFrequency = TimeSpan.FromSeconds(30),
            };
            containerCache = new MemoryCache(memOptions);
            this.logger = logger;

            storageTransferOptions = new StorageTransferOptions()
            {
                InitialTransferSize = initialTransferSize,
                MaximumConcurrency = maxConcurrency,
                MaximumTransferSize = maxTransferSize,
            };
        }

        #endregion

        #region Container Methods

        /// <summary>
        /// Deletes a blob storage container if it exists.
        /// </summary>
        /// <param name="containerName">Name of container to delete.</param>
        /// <param name="conditions">Optional blob lease access conditions for a container or blob.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if container deleted or did not exist; otherwise false.</returns>
        public async Task<bool> DeleteContainerIfExistsAsync(string containerName, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            containerClient = blobService.GetBlobContainerClient(containerName);
            Response<bool> response = await containerClient.DeleteIfExistsAsync(conditions, cancellationToken);
            logger?.LogTrace(new EventId(91010, "StorageBlob.DeleteContainerIfExistsAsync"), "Container {ContainerName} deleted if exists.", containerName);
            return response.Value;
        }

        /// <summary>
        /// Creates a blob storage container if it does not already exist.
        /// </summary>
        /// <param name="containerName">Name of container to create.</param>
        /// <param name="publicAccess">Specifies whether data in the container may be accessed publicly and the level of access.; default is "None".</param>
        /// <returns>BlobContainerInfo</returns>
        public async Task<Response<BlobContainerInfo>> CreateContainerIfNotExistsAsync(string containerName, PublicAccessType publicAccess = PublicAccessType.None)
        {
            containerClient = blobService.GetBlobContainerClient(containerName);
            Response<BlobContainerInfo> response = await containerClient.CreateIfNotExistsAsync(publicAccess);
            logger?.LogTrace(new EventId(91020, "StorageBlob.CreateContainerIfNotExistsAsync"), "Container {ContainerName} created if exists.", containerName);
            return response;
        }

        /// <summary>
        /// Gets a list of pageable container names.
        /// </summary>
        /// <param name="traits">Optional specifies trait options for shaping the blob containers.; default is "None".</param>
        /// <param name="states">Optional specifies states options for shaping the blob containers; default is "None".</param>
        /// <param name="prefix">Optional specifies a string that filters the results to return only containers whose name begins with the specified prefix.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>AsyncPageable&lt;BlobContainerItem&gt;</returns>
        public AsyncPageable<BlobContainerItem> ListContainers(BlobContainerTraits traits = BlobContainerTraits.None, BlobContainerStates states = BlobContainerStates.None, string prefix = null, CancellationToken cancellationToken = default)
        {
            AsyncPageable<BlobContainerItem> pages = blobService.GetBlobContainersAsync(traits, states, prefix, cancellationToken);
            logger?.LogTrace(new EventId(91030, "StorageBlob.ListContainers"), "List containers succeeded.");
            return pages;
        }

        /// <summary>
        /// Indicates whether a blob container exists.
        /// </summary>
        /// <param name="containerName">Name of container.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if container exists; otherwise false.</returns>
        public async Task<bool> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default)
        {
            containerClient = blobService.GetBlobContainerClient(containerName);
            Response<bool> response = await containerClient.ExistsAsync(cancellationToken);
            logger?.LogTrace(new EventId(91040, "StorageBlob.ContainerExistsAsync"), "Container {ContainerName} exists = {Exists}.", containerName, response.Value);
            return response.Value;
        }

        /// <summary>
        /// Gets a list of blob metadata in a container.
        /// </summary>
        /// <param name="containerName">Name of container to get metadata.</param>
        /// <param name="traits">Optional specifies trait options for shaping the blob containers.; default is "None".</param>
        /// <param name="states">Optional specifies states options for shaping the blob containers; default is "None".</param>
        /// <param name="prefix">Optional specifies a string that filters the results to return only containers whose name begins with the specified prefix.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Array of Dictionary&lt;string,string&gt;[] containing metadata. </returns>
        public async Task<IDictionary<string, string>[]> ListBlobMetadataInContainerAsync(string containerName, BlobTraits traits = BlobTraits.None, BlobStates states = BlobStates.None, string prefix = null, CancellationToken cancellationToken = default)
        {
            List<IDictionary<string, string>> list = new();
            containerClient = blobService.GetBlobContainerClient(containerName);

            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(traits, states, prefix, cancellationToken))
            {
                list.Add(blobItem.Metadata);
            }

            logger?.LogTrace(new EventId(91050, "StorageBlob.ListBlobMetadataInContainerAsync"), "Container {ContainerName} has {Count} metadata listings.", containerName, list.Count);
            return list.ToArray();
        }

        #endregion

        #region Read Methods

        /// <summary>
        /// Gets a list of blob names in a storage container.
        /// </summary>
        /// <param name="containerName">Name of container to return blob names.</param>
        /// <param name="segmentSize">Optional maximum number of blob names to be returned.</param>
        /// <param name="traits">Optional specifies trait options for shaping the blob containers.; default is "None".</param>
        /// <param name="states">Optional specifies states options for shaping the blob containers; default is "None".</param>
        /// <param name="prefix">Optional specifies a string that filters the results to return only containers whose name begins with the specified prefix.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>List of blob names in container.</returns>
        public async Task<List<string>> ListBlobsAsync(string containerName, int? segmentSize = null, BlobTraits traits = BlobTraits.None, BlobStates states = BlobStates.None, string prefix = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            IAsyncEnumerable<Page<BlobItem>> result = containerClient.GetBlobsAsync(traits, states, prefix, cancellationToken)
                .AsPages(default, segmentSize);

            List<string> blobNames = new();
            await foreach (Page<BlobItem> blobPage in result)
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {
                    blobNames.Add(blobItem.Name);
                }
            }

            logger?.LogTrace(new EventId(91060, "StorageBlob.ListBlobsAsync"), "Container {ContainerName} has {Count} blobs.", containerName, blobNames.Count);
            return blobNames;
        }

        /// <summary>
        /// Gets the contents of a blob and returns as an array of bytes.
        /// </summary>
        /// <param name="containerName">Name of container where blob resides.</param>
        /// <param name="blobName">Name of the blob to read.</param>
        /// <param name="options">Optional blob open read options.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Array of bytes of the blob content.</returns>
        public async Task<byte[]> ReadBlockBlobAsync(string containerName, string blobName, BlobOpenReadOptions options = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            BlockBlobClient blockBlobClient = containerClient.GetBlockBlobClient(blobName);
            options ??= new BlobOpenReadOptions(true);
            Stream stream = await blockBlobClient.OpenReadAsync(options, cancellationToken);
            byte[] buffer = new byte[stream.Length];
            _ = await stream.ReadAsync(buffer, cancellationToken);
            await stream.DisposeAsync();
            logger?.LogTrace(new EventId(91070, "StorageBlob.ReadBlockBlobAsync"), "Container {ContainerName} blob {BlobName} read.", containerName, blobName);
            return buffer;
        }

        /// <summary>
        /// Gets the contents of an append blob and returns as an array of bytes.
        /// </summary>
        /// <param name="containerName">Name of container where blob resides.</param>
        /// <param name="blobName">Name of the blob to read.</param>
        /// <param name="options">Optional blob open read options.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Array of bytes of the blob content.</returns>
        public async Task<byte[]> ReadAppendBlobAsync(string containerName, string blobName, BlobOpenReadOptions options = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            AppendBlobClient appendBlobClient = containerClient.GetAppendBlobClient(blobName);
            options ??= new BlobOpenReadOptions(true);
            Stream stream = await appendBlobClient.OpenReadAsync(options, cancellationToken);
            byte[] buffer = new byte[stream.Length];
            _ = await stream.ReadAsync(buffer, cancellationToken);
            await stream.DisposeAsync();
            logger?.LogTrace(new EventId(91080, "StorageBlob.ReadAppendBlobAsync"), "Container {ContainerName} blob {BlobName} read.", containerName, blobName);
            return buffer;
        }

        /// <summary>
        /// Gets the contents of a page blob and returns as an array of bytes.
        /// </summary>
        /// <param name="containerName">Name of container where blob resides.</param>
        /// <param name="blobName">Name of the blob to read.</param>
        /// <param name="options">Optional blob open read options.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Array of bytes of the blob content.</returns>
        public async Task<byte[]> ReadPageBlobAsync(string containerName, string blobName, BlobOpenReadOptions options = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            PageBlobClient pageBlobClient = containerClient.GetPageBlobClient(blobName);
            options ??= new BlobOpenReadOptions(true);
            Stream stream = await pageBlobClient.OpenReadAsync(options, cancellationToken);
            byte[] buffer = new byte[stream.Length];
            _ = await stream.ReadAsync(buffer, cancellationToken);
            await stream.DisposeAsync();
            logger?.LogTrace(new EventId(91090, "StorageBlob.ReadPageBlobAsync"), "Container {ContainerName} blob {BlobName} read.", containerName, blobName);
            return buffer;
        }

        #endregion

        #region Write Methods

        /// <summary>
        /// Writes a block blob to a container.
        /// </summary>
        /// <param name="containerName">Name of container where blob is written.</param>
        /// <param name="blobName">Name of blob to write.</param>
        /// <param name="contentType">Content type of blob.</param>
        /// <param name="content">Content of the blob to write.</param>
        /// <param name="options">Optional block blob open write options.</param>
        /// <param name="metadata">Optional metadata for the blob.</param>
        /// <param name="conditions">Optional blob lease access conditions for a container or blob.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task</returns>
        public async Task WriteBlockBlobAsync(string containerName, string blobName, string contentType, byte[] content, BlockBlobOpenWriteOptions options = null, IDictionary<string, string> metadata = null, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            BlockBlobClient blockBlobClient = containerClient.GetBlockBlobClient(blobName);

            options ??= new BlockBlobOpenWriteOptions() { HttpHeaders = new BlobHttpHeaders() { ContentType = contentType } };
            options.HttpHeaders ??= new BlobHttpHeaders() { ContentType = contentType };
            options.HttpHeaders.ContentType ??= contentType;

            using Stream stream = await blockBlobClient.OpenWriteAsync(true, options, cancellationToken);
            await stream.WriteAsync(content, cancellationToken);
            await stream.FlushAsync(cancellationToken);
            await stream.DisposeAsync();

            if (metadata != null)
            {
                await blockBlobClient.SetMetadataAsync(metadata, conditions, cancellationToken);
            }

            logger?.LogTrace(new EventId(91100, "StorageBlob.WriteBlockBlobAsync"), "Container {ContainerName} blob {BlobName} written.", containerName, blobName);
        }

        /// <summary>
        /// Writes a block blob to a container.
        /// </summary>
        /// <param name="containerName">Name of container where blob is written.</param>
        /// <param name="blobName">Name of blob to write.</param>
        /// <param name="contentType">Content type of blob.</param>
        /// <param name="content">Content stream of the blob to write.</param>
        /// <param name="options">Optional block blob open write options.</param>
        /// <param name="metadata">Optional metadata for the blob.</param>
        /// <param name="conditions">Optional blob lease access conditions for a container or blob.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task</returns>
        public async Task WriteBlockBlobAsync(string containerName, string blobName, string contentType, Stream content, BlockBlobOpenWriteOptions options = null, IDictionary<string, string> metadata = null, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[content.Length];
            await content.ReadAsync(buffer, cancellationToken);
            await WriteBlockBlobAsync(containerName, blobName, contentType, buffer, options, metadata, conditions, cancellationToken);
            logger?.LogTrace(new EventId(91110, "StorageBlob.WriteBlockBlobAsync"), "Container {ContainerName} blob {BlobName} written.", containerName, blobName);
        }

        /// <summary>
        /// Writes an append blob to a storage container.
        /// </summary>
        /// <param name="containerName">Name of container where blob is written.</param>
        /// <param name="blobName">Name of blob to write.</param>
        /// <param name="contentType">Content type of blob.</param>
        /// <param name="content">Content of the blob to write.</param>
        /// <param name="writeOptions">Optional append blob open write options.</param>
        /// <param name="createOptions">Optional append blob create options.</param>
        /// <param name="metadata">Optional blob metadata.</param>
        /// <param name="conditions">Optional blob lease access conditions for a container or blob.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task</returns>
        public async Task WriteAppendBlobAsync(string containerName, string blobName, string contentType, byte[] content, AppendBlobOpenWriteOptions writeOptions = null, AppendBlobCreateOptions createOptions = null, IDictionary<string, string> metadata = null, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            AppendBlobClient appendBlobClient = containerClient.GetAppendBlobClient(blobName);

            createOptions ??= new AppendBlobCreateOptions() { HttpHeaders = new BlobHttpHeaders() { ContentType = contentType } };
            createOptions.HttpHeaders ??= new BlobHttpHeaders() { ContentType = contentType };
            createOptions.HttpHeaders.ContentType ??= contentType;

            _ = await appendBlobClient.CreateIfNotExistsAsync(createOptions, cancellationToken);
            Stream stream = await appendBlobClient.OpenWriteAsync(false, writeOptions, cancellationToken);
            await stream.WriteAsync(content, cancellationToken);
            await stream.FlushAsync(cancellationToken);
            await stream.DisposeAsync();

            if (metadata != null)
            {
                await appendBlobClient.SetMetadataAsync(metadata, conditions, cancellationToken);
            }

            logger?.LogTrace(new EventId(91120, "StorageBlob.WriteAppendBlobAsync"), "Container {ContainerName} blob {BlobName} written.", containerName, blobName);
        }

        /// <summary>
        /// Writes an append blob to a storage container.
        /// </summary>
        /// <param name="containerName">Name of container where blob is written.</param>
        /// <param name="blobName">Name of blob to write.</param>
        /// <param name="contentType">Content type of blob.</param>
        /// <param name="content">Content stream of the blob to write.</param>
        /// <param name="writeOptions">Optional append blob open write options.</param>
        /// <param name="createOptions">Optional append blob create options.</param>
        /// <param name="metadata">Optional blob metadata.</param>
        /// <param name="conditions">Optional blob lease access conditions for a container or blob.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task</returns>
        public async Task WriteAppendBlobAsync(string containerName, string blobName, string contentType, Stream content, AppendBlobOpenWriteOptions writeOptions = null, AppendBlobCreateOptions createOptions = null, IDictionary<string, string> metadata = null, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[content.Length];
            await content.ReadAsync(buffer, cancellationToken);
            await WriteAppendBlobAsync(containerName, blobName, contentType, buffer, writeOptions, createOptions, metadata, conditions, cancellationToken);
            logger?.LogTrace(new EventId(91130, "StorageBlob.WriteAppendBlobAsync"), "Container {ContainerName} blob {BlobName} written.", containerName, blobName);
        }

        #endregion

        #region Upload Methods

        /// <summary>
        /// Uploads a blob from the local file system to a blob storage container.
        /// </summary>
        /// <param name="containerName">Name of container where blob is written.</param>
        /// <param name="blobName">Name of blob to write.</param>
        /// <param name="contentType">Content type of blob.</param>
        /// <param name="path">Path to local file to upload.</param>
        /// <param name="metadata">Optional blob metadata.</param>
        /// <param name="conditions">Optional blob lease access conditions for a container or blob.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task</returns>
        public async Task UploadBlobAsync(string containerName, string blobName, string contentType, string path, IDictionary<string, string> metadata = null, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            BlobUploadOptions options = new()
            {
                HttpHeaders = new BlobHttpHeaders()
                {
                    ContentType = contentType,
                },
                TransferOptions = storageTransferOptions,
            };

            _ = await blobClient.UploadAsync(path, options, cancellationToken);

            if (metadata != null)
            {
                await blobClient.SetMetadataAsync(metadata, conditions, cancellationToken);
            }

            logger?.LogTrace(new EventId(91140, "StorageBlob.UploadBlobAsync"), "Container {ContainerName} blob {BlobName} uploaded.", containerName, blobName);
        }

        #endregion

        #region Download Methods

        /// <summary>
        /// Downloads a block blob from a container and returns the result.
        /// </summary>
        /// <param name="containerName">Name of container where blob exists.</param>
        /// <param name="blobName">Name of blob to read.</param>
        /// <param name="conditions">Optional blob lease access conditions for a container or blob.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>BlobDownloadResult</returns>
        public async Task<BlobDownloadResult> DownloadBlockBlobAsync(string containerName, string blobName, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            BlockBlobClient blockBlobClient = containerClient.GetBlockBlobClient(blobName);

            BlobDownloadResult result = await blockBlobClient.DownloadContentAsync(conditions, cancellationToken);
            logger?.LogTrace(new EventId(91150, "StorageBlob.DownloadBlockBlobAsync"), "Container {ContainerName} blob {BlobName} downloaded.", containerName, blobName);
            return result;
        }

        /// <summary>
        /// Downloads a block blob and writes a file to the local file system.
        /// </summary>
        /// <param name="containerName">Name of container where blob exists.</param>
        /// <param name="blobName">Name of blob to read.</param>
        /// <param name="path">Path to write the downloaded blob to the local file system.</param>
        /// <param name="conditions">Optional blob lease access conditions for a container or blob.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Response</returns>
        public async Task<Response> DownloadBlockBlobToAsync(string containerName, string blobName, string path, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            BlockBlobClient blockBlobClient = containerClient.GetBlockBlobClient(blobName);
            Response response = await blockBlobClient.DownloadToAsync(path, conditions, storageTransferOptions, cancellationToken);
            logger?.LogTrace(new EventId(91160, "StorageBlob.DownloadBlockBlobToAsync"), "Container {ContainerName} blob {BlobName} downloaded to {Path}.", containerName, blobName, path);
            return response;
        }

        /// <summary>
        /// Downloads a block blob from blob storage and returns the response.
        /// </summary>
        /// <param name="containerName">Name of container where blob exists.</param>
        /// <param name="blobName">Name of blob to read.</param>
        /// <param name="destination">Stream to write.</param>
        /// <param name="conditions">Optional blob lease access conditions for a container or blob.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Response</returns>
        public async Task<Response> DownloadBlockBlobToAsync(string containerName, string blobName, Stream destination, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            BlockBlobClient blockBlobClient = containerClient.GetBlockBlobClient(blobName);
            Response response = await blockBlobClient.DownloadToAsync(destination, conditions, storageTransferOptions, cancellationToken);
            logger?.LogTrace(new EventId(91170, "StorageBlob.DownloadBlockBlobToAsync"), "Container {ContainerName} blob {BlobName} downloaded to stream.", containerName, blobName);
            return response;
        }

        /// <summary>
        /// Downloads an append blob from a container and returns the result.
        /// </summary>
        /// <param name="containerName">Name of container where blob exists.</param>
        /// <param name="blobName">Name of blob to read.</param>
        /// <param name="conditions">Optional blob lease access conditions for a container or blob.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>BlobDownloadResult</returns>
        public async Task<BlobDownloadResult> DownloadAppendBlobAsync(string containerName, string blobName, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            AppendBlobClient appendBlobClient = containerClient.GetAppendBlobClient(blobName);
            Response<BlobDownloadResult> response = await appendBlobClient.DownloadContentAsync(conditions, cancellationToken);
            logger?.LogTrace(new EventId(91180, "StorageBlob.DownloadAppendBlobAsync"), "Container {ContainerName} blob {BlobName} downloaded.", containerName, blobName);
            return response.Value;
        }

        /// <summary>
        /// Downloads an append blob from a container and returns the result.
        /// </summary>
        /// <param name="containerName">Name of container where blob exists.</param>
        /// <param name="blobName">Name of blob to read.</param>
        /// <param name="path">Path to write the downloaded blob to the local file system.</param>
        /// <param name="conditions">Optional blob lease access conditions for a container or blob.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Response</returns>
        public async Task<Response> DownloadAppendBlobToAsync(string containerName, string blobName, string path, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            AppendBlobClient appendBlobClient = containerClient.GetAppendBlobClient(blobName);
            Response response = await appendBlobClient.DownloadToAsync(path, conditions, storageTransferOptions, cancellationToken);
            logger?.LogTrace(new EventId(91190, "StorageBlob.DownloadAppendBlobToAsync"), "Container {ContainerName} blob {BlobName} downloaded to {Path}.", containerName, blobName, path);
            return response;
        }

        /// <summary>
        /// Downloads an append blob from blob storage and returns the response.
        /// </summary>
        /// <param name="containerName">Name of container where blob exists.</param>
        /// <param name="blobName">Name of blob to read.</param>
        /// <param name="destination">Stream to write.</param>
        /// <param name="conditions">Optional blob lease access conditions for a container or blob.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Response</returns>
        public async Task<Response> DownloadAppendBlobToAsync(string containerName, string blobName, Stream destination, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            AppendBlobClient appendBlobClient = containerClient.GetAppendBlobClient(blobName);
            Response response = await appendBlobClient.DownloadToAsync(destination, conditions, storageTransferOptions, cancellationToken);
            logger?.LogTrace(new EventId(91200, "StorageBlob.DownloadAppendBlobToAsync"), "Container {ContainerName} blob {BlobName} downloaded to stream.", containerName, blobName);
            return response;
        }

        #endregion

        #region Delete Methods

        /// <summary>
        /// Deletes a blob from a container in blob storage.
        /// </summary>
        /// <param name="containerName">Name of container where blob is written.</param>
        /// <param name="blobName">Name of blob to delete.</param>
        /// <returns>True if blob is deleted; otherwise false.</returns>
        public async Task<bool> DeleteBlobAsync(string containerName, string blobName)
        {
            BlobContainerClient containerClient = blobService.GetBlobContainerClient(containerName);
            Response<bool> response = await containerClient.DeleteBlobIfExistsAsync(blobName);
            logger?.LogTrace(new EventId(91210, "StorageBlob.DeleteBlobAsync"), "Container {ContainerName} blob {BlobName} deleted.", containerName, blobName);
            return response.Value;
        }

        #endregion

        #region Get Properties Methods

        /// <summary>
        /// Gets properties of a blob in a container.
        /// </summary>
        /// <param name="containerName">Name of container where blob exists.</param>
        /// <param name="blobName">Name of blob to get properties.</param>
        /// <param name="conditions">Optional blob lease access conditions for a container or blob.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>BlobProperties</returns>
        public async Task<BlobProperties> GetBlobPropertiesAsync(string containerName, string blobName, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            Response<BlobProperties> result = await blobClient.GetPropertiesAsync(conditions, cancellationToken);
            logger?.LogTrace(new EventId(91220, "StorageBlob.GetBlobPropertiesAsync"), "Got properties client for container {ContainerName} and blob {BlobName}.", containerName, blobName);
            return result.Value;
        }

        #endregion

        #region Private Methods
        private BlobContainerClient GetContainerClient(string containerName)
        {
            if (!containerCache.TryGetValue<BlobContainerClient>(containerName, out BlobContainerClient client))
            {
                client = blobService.GetBlobContainerClient(containerName);
                containerCache.Set<BlobContainerClient>(containerName, client, _cacheOptions);
            }

            if (client == null)
            {
                Exception ex = new NullReferenceException("Blob container client.");
                logger?.LogError(ex, "Blob container client.");
                throw ex;
            }

            logger?.LogTrace(new EventId(91026, "StorageBlob.GetContainerClient"), "Got container client for container {ContainerName}.", containerName);
            return client;
        }

        #endregion

    }
}
