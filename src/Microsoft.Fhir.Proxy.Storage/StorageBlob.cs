using Azure;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Storage
{
    public class StorageBlob
    {
        #region ctor
        public StorageBlob(string connectionString, long? initialTransferSize = null, int? maxConcurrency = null, int? maxTransferSize = null, ILogger logger = null)
            : this(initialTransferSize, maxConcurrency, maxTransferSize, logger)
        {
            blobService = new BlobServiceClient(connectionString);
        }

        public StorageBlob(string connectionString, BlobClientOptions options, long? initialTransferSize = null, int? maxConcurrency = null, int? maxTransferSize = null, ILogger logger = null)
            : this(initialTransferSize, maxConcurrency, maxTransferSize, logger)
        {
            blobService = new BlobServiceClient(connectionString, options);
        }

        public StorageBlob(Uri serviceUri, BlobClientOptions options = null, long? initialTransferSize = null, int? maxConcurrency = null, int? maxTransferSize = null, ILogger logger = null)
            : this(initialTransferSize, maxConcurrency, maxTransferSize, logger)
        {
            blobService = new BlobServiceClient(serviceUri, options);
        }

        public StorageBlob(Uri serviceUri, TokenCredential credentials, BlobClientOptions options = null, long? initialTransferSize = null, int? maxConcurrency = null, int? maxTransferSize = null, ILogger logger = null)
            : this(initialTransferSize, maxConcurrency, maxTransferSize, logger)
        {
            blobService = new BlobServiceClient(serviceUri, credentials, options);
        }

        public StorageBlob(Uri serviceUri, AzureSasCredential credential, BlobClientOptions options = null, long? initialTransferSize = null, int? maxConcurrency = null, int? maxTransferSize = null, ILogger logger = null)
             : this(initialTransferSize, maxConcurrency, maxTransferSize, logger)
        {
            blobService = new BlobServiceClient(serviceUri, credential, options);
        }

        public StorageBlob(Uri serviceUri, StorageSharedKeyCredential credential, BlobClientOptions options = null, long? initialTransferSize = null, int? maxConcurrency = null, int? maxTransferSize = null, ILogger logger = null)
             : this(initialTransferSize, maxConcurrency, maxTransferSize, logger)
        {
            blobService = new BlobServiceClient(serviceUri, credential, options);
        }

        public StorageBlob(long? initialTransferSize, int? maxConcurrency, int? maxTransferSize, ILogger logger = null)
        {
            cacheOptions = new MemoryCacheEntryOptions();
            cacheOptions.SetSlidingExpiration(TimeSpan.FromSeconds(300));
            MemoryCacheOptions memOptions = new();
            memOptions.ExpirationScanFrequency = TimeSpan.FromSeconds(30);
            containerCache = new MemoryCache(memOptions);
            this.logger = logger;

            storageTransferOptions = new StorageTransferOptions()
            {
                InitialTransferSize = initialTransferSize,
                MaximumConcurrency = maxConcurrency,
                MaximumTransferSize = maxTransferSize
            };
        }

        #endregion

        private readonly BlobServiceClient blobService;

        private BlobContainerClient containerClient;

        private StorageTransferOptions storageTransferOptions;

        private readonly MemoryCacheEntryOptions cacheOptions = new();

        private readonly MemoryCache containerCache;

        private readonly ILogger logger;

        #region Container Methods


        public async Task<bool> DeleteContainerIfExistsAsync(string containerName, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            containerClient = blobService.GetBlobContainerClient(containerName);
            Response<bool> response = await containerClient.DeleteIfExistsAsync(conditions, cancellationToken);
            logger?.LogTrace(new EventId(91000, "StorageBlob.DeleteContainerIfExistsAsync"), $"Container {containerName} deleted if exists.");
            return response.Value;
        }

        public async Task<Response<BlobContainerInfo>> CreateContainerIfNotExistsAsync(string containerName, PublicAccessType publicAccess = PublicAccessType.None)
        {
            containerClient = blobService.GetBlobContainerClient(containerName);
            Response<BlobContainerInfo> response = await containerClient.CreateIfNotExistsAsync(publicAccess);
            logger?.LogTrace(new EventId(91001, "StorageBlob.CreateContainerIfNotExistsAsync"), $"Container {containerName} created if exists.");
            return response;
        }


        public AsyncPageable<BlobContainerItem> ListContainers()
        {
            AsyncPageable<BlobContainerItem> pages = blobService.GetBlobContainersAsync();
            logger?.LogTrace(new EventId(91002, "StorageBlob.ListContainers"), $"List containers succeeded.");
            return pages;
        }

        public async Task<bool> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default)
        {
            containerClient = blobService.GetBlobContainerClient(containerName);
            Response<bool> response = await containerClient.ExistsAsync(cancellationToken);
            logger?.LogTrace(new EventId(91003, "StorageBlob.ContainerExistsAsync"), $"Container {containerName} exists = {response.Value}.");
            return response.Value;
        }



        #endregion

        #region Read Methods

        public async Task<List<string>> ListBlobsAsync(string containerName, int? segmentSize = null)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            var result = containerClient.GetBlobsAsync()
                .AsPages(default, segmentSize);

            List<string> blobNames = new();
            await foreach (Azure.Page<BlobItem> blobPage in result)
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {
                    blobNames.Add(blobItem.Name);
                }
            }

            return blobNames;
        }


        public async Task<byte[]> ReadBlockBlobAsync(string containerName, string blobName, BlobOpenReadOptions options, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            BlockBlobClient blockBlobClient = containerClient.GetBlockBlobClient(blobName);

            Stream stream = await blockBlobClient.OpenReadAsync(options, cancellationToken);
            byte[] buffer = new byte[stream.Length];
            _ = await stream.ReadAsync(buffer, cancellationToken);
            await stream.DisposeAsync();
            logger?.LogTrace(new EventId(91004, "StorageBlob.ReadBlockBlobAsync"), $"Container {containerName} blob {blobName} read.");
            return buffer;
        }

        public async Task<byte[]> ReadAppendBlobAsync(string containerName, string blobName, BlobOpenReadOptions options, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            AppendBlobClient appendBlobClient = containerClient.GetAppendBlobClient(blobName);

            Stream stream = await appendBlobClient.OpenReadAsync(options, cancellationToken);
            byte[] buffer = new byte[stream.Length];
            _ = await stream.ReadAsync(buffer, cancellationToken);
            await stream.DisposeAsync();
            logger?.LogTrace(new EventId(91005, "StorageBlob.ReadAppendBlobAsync"), $"Container {containerName} blob {blobName} read.");
            return buffer;
        }

        public async Task<byte[]> ReadPageBlobAsync(string containerName, string blobName, BlobOpenReadOptions options, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            PageBlobClient pageBlobClient = containerClient.GetPageBlobClient(blobName);

            Stream stream = await pageBlobClient.OpenReadAsync(options, cancellationToken);
            byte[] buffer = new byte[stream.Length];
            _ = await stream.ReadAsync(buffer, cancellationToken);
            await stream.DisposeAsync();
            logger?.LogTrace(new EventId(91006, "StorageBlob.ReadPageBlobAsync"), $"Container {containerName} blob {blobName} read.");
            return buffer;
        }

        #endregion

        #region Write Methods
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

            logger?.LogTrace(new EventId(91007, "StorageBlob.WriteBlockBlobAsync"), $"Container {containerName} blob {blobName} written.");
        }

        public async Task WriteBlockBlobAsync(string containerName, string blobName, string contentType, Stream content, BlockBlobOpenWriteOptions options = null, IDictionary<string, string> metadata = null, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[content.Length];
            await content.ReadAsync(buffer, cancellationToken);
            await WriteBlockBlobAsync(containerName, blobName, contentType, buffer, options, metadata, conditions, cancellationToken);
            logger?.LogTrace(new EventId(91008, "StorageBlob.WriteBlockBlobAsync"), $"Container {containerName} blob {blobName} written.");
        }

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

            logger?.LogTrace(new EventId(91009, "StorageBlob.WriteAppendBlobAsync"), $"Container {containerName} blob {blobName} written.");
        }

        public async Task WriteAppendBlobAsync(string containerName, string blobName, string contentType, Stream content, AppendBlobOpenWriteOptions writeOptions = null, AppendBlobCreateOptions createOptions = null, IDictionary<string, string> metadata = null, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[content.Length];
            await content.ReadAsync(buffer, cancellationToken);
            await WriteAppendBlobAsync(containerName, blobName, contentType, buffer, writeOptions, createOptions, metadata, conditions, cancellationToken);
            logger?.LogTrace(new EventId(91010, "StorageBlob.WriteAppendBlobAsync"), $"Container {containerName} blob {blobName} written.");
        }

        #endregion

        #region Upload Methods

        public async Task UploadBlobAsync(string containerName, string blobName, string contentType, string path, IDictionary<string, string> metadata = null, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            BlobUploadOptions options = new()
            {
                HttpHeaders = new BlobHttpHeaders()
                {
                    ContentType = contentType
                },
                TransferOptions = storageTransferOptions

            };
            _ = await blobClient.UploadAsync(path, options, cancellationToken);


            if (metadata != null)
            {
                await blobClient.SetMetadataAsync(metadata, conditions, cancellationToken);
            }

            logger?.LogTrace(new EventId(91013, "StorageBlob.WritePageBlobAsync"), $"Container {containerName} blob {blobName} uploaded.");
        }

        #endregion

        #region Download Methods
        public async Task<BlobDownloadResult> DownloadBlockBlobAsync(string containerName, string blobName, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            BlockBlobClient blockBlobClient = containerClient.GetBlockBlobClient(blobName);

            BlobDownloadResult result = await blockBlobClient.DownloadContentAsync(conditions, cancellationToken);
            logger?.LogTrace(new EventId(91014, "StorageBlob.DownloadBlockBlobAsync"), $"Container {containerName} blob {blobName} downloaded.");
            return result;
        }

        public async Task<Response> DownloadBlockBlobToAsync(string containerName, string blobName, string path, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            BlockBlobClient blockBlobClient = containerClient.GetBlockBlobClient(blobName);
            Response response = await blockBlobClient.DownloadToAsync(path, conditions, storageTransferOptions, cancellationToken);
            logger?.LogTrace(new EventId(91015, "StorageBlob.DownloadBlockBlobToAsync"), $"Container {containerName} blob {blobName} downloaded to {path}.");
            return response;
        }

        public async Task<Response> DownloadBlockBlobToAsync(string containerName, string blobName, Stream destination, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            BlockBlobClient blockBlobClient = containerClient.GetBlockBlobClient(blobName);
            Response response = await blockBlobClient.DownloadToAsync(destination, conditions, storageTransferOptions, cancellationToken);
            logger?.LogTrace(new EventId(91016, "StorageBlob.DownloadBlockBlobToAsync"), $"Container {containerName} blob {blobName} downloaded to stream.");
            return response;
        }

        public async Task<BlobDownloadResult> DownloadAppendBlobAsync(string containerName, string blobName, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            AppendBlobClient appendBlobClient = containerClient.GetAppendBlobClient(blobName);
            Response<BlobDownloadResult> response = await appendBlobClient.DownloadContentAsync(conditions, cancellationToken);
            logger?.LogTrace(new EventId(91017, "StorageBlob.DownloadAppendBlobAsync"), $"Container {containerName} blob {blobName} downloaded.");
            return response.Value;
        }

        public async Task<Response> DownloadAppendBlobToAsync(string containerName, string blobName, string path, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            AppendBlobClient appendBlobClient = containerClient.GetAppendBlobClient(blobName);
            Response response = await appendBlobClient.DownloadToAsync(path, conditions, storageTransferOptions, cancellationToken);
            logger?.LogTrace(new EventId(91018, "StorageBlob.DownloadAppendBlobToAsync"), $"Container {containerName} blob {blobName} downloaded to {path}.");
            return response;
        }

        public async Task<Response> DownloadAppendBlobToAsync(string containerName, string blobName, Stream destination, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            AppendBlobClient appendBlobClient = containerClient.GetAppendBlobClient(blobName);
            Response response = await appendBlobClient.DownloadToAsync(destination, conditions, storageTransferOptions, cancellationToken);
            logger?.LogTrace(new EventId(91019, "StorageBlob.DownloadAppendBlobToAsync"), $"Container {containerName} blob {blobName} downloaded to stream.");
            return response;
        }

        #endregion

        #region Delete Methods

        public async Task<bool> DeleteBlobAsync(string containerName, string blobName)
        {
            BlobContainerClient containerClient = blobService.GetBlobContainerClient(containerName);
            Response<bool> response = await containerClient.DeleteBlobIfExistsAsync(blobName);
            logger?.LogTrace(new EventId(91024, "StorageBlob.DeleteBlobAsync"), $"Container {containerName} blob {blobName} deleted.");
            return response.Value;
        }

        #endregion

        #region Get Properties Methods
        public async Task<BlobProperties> GetBlobPropertiesAsync(string containerName, string blobName, BlobRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            BlobContainerClient containerClient = GetContainerClient(containerName.ToLowerInvariant());
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            Response<BlobProperties> result = await blobClient.GetPropertiesAsync(conditions, cancellationToken);
            logger?.LogTrace(new EventId(91025, "StorageBlob.GetBlobPropertiesAsync"), $"Got properties client for container {containerName} and blob {blobName}.");
            return result.Value;
        }

        #endregion

        #region Private Methods
        private BlobContainerClient GetContainerClient(string containerName)
        {
            if (!containerCache.TryGetValue<BlobContainerClient>(containerName, out BlobContainerClient client))
            {
                client = blobService.GetBlobContainerClient(containerName);
                containerCache.Set<BlobContainerClient>(containerName, client, cacheOptions);
            }

            if (client == null)
            {
                Exception ex = new NullReferenceException("Blob container client.");
                logger?.LogError(ex, "Blob container client.");
                throw ex;
            }

            logger?.LogTrace(new EventId(91026, "StorageBlob.GetContainerClient"), $"Got container client for container {containerName}.");
            return client;
        }

        #endregion

    }
}
