using Azure;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Storage
{
    public class StorageFiles
    {
        private readonly ShareServiceClient serviceClient;
        private readonly ILogger logger;

        #region ctor
        public StorageFiles(string connectionString, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new ShareServiceClient(connectionString);
        }

        public StorageFiles(string connectionString, ShareClientOptions options, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new ShareServiceClient(connectionString, options);
        }

        public StorageFiles(Uri serviceUri, StorageSharedKeyCredential credentials, ShareClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new ShareServiceClient(serviceUri, credentials, options);
        }

        public StorageFiles(Uri serviceUri, ShareClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new ShareServiceClient(serviceUri, options);
        }

        public StorageFiles(Uri serviceUri, AzureSasCredential credentials, ShareClientOptions options, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new ShareServiceClient(serviceUri, credentials, options);
        }

        protected StorageFiles(ILogger logger = null)
        {
            this.logger = logger;
        }

        #endregion

        public async Task<ShareInfo> CreateShareIfNotExistsAsync(string shareName, ShareCreateOptions options = null, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            Response<ShareInfo> info = await shareClient.CreateIfNotExistsAsync(options, cancellationToken);
            logger?.LogTrace(new EventId(93000, "StorageFile.CreateShareIfNotExistsAsync"), $"File share {shareName} created.");
            return info?.Value;
        }

        public async Task<bool> DeleteShareIfExistsAsync(string shareName, ShareDeleteOptions options = null, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            bool result = await shareClient.DeleteIfExistsAsync(options, cancellationToken);
            logger?.LogTrace(new EventId(93010, "StorageFile.DeleteShareIfExistsAsync"), $"File share {shareName} deleted {result}.");
            return result;
        }

        public async Task<ShareDirectoryInfo> CreateDirectoryIfNotExistsAsync(string shareName, string directoryName, IDictionary<string, string> metadata = null, FileSmbProperties smbProperties = null, string filePermission = null, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            var dirClient = shareClient.GetDirectoryClient(directoryName);
            Response<ShareDirectoryInfo> result = await dirClient.CreateIfNotExistsAsync(metadata, smbProperties, filePermission, cancellationToken);
            logger?.LogTrace(new EventId(93020, "StorageFile.CreateDirectoryIfNotExistsAsync"), $"Directory {directoryName} created on file share {shareName}.");
            return result.Value;
        }

        public async Task<bool> DeleteDirectoryIfExistsAsync(string shareName, string directoryName, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            var dirClient = shareClient.GetDirectoryClient(directoryName);
            Response<bool> response = await dirClient.DeleteIfExistsAsync(cancellationToken);
            logger?.LogTrace(new EventId(93030, "StorageFile.DeleteDirectoryIfExistsAsync"), $"Directory {directoryName} on file share {shareName} deleted {response.Value}.");
            return response.Value;
        }

        public async Task WriteFileAsync(string shareName, string directoryName, string fileName, byte[] content, ShareFileOpenWriteOptions options = null, CancellationToken cancellationToken = default)
        {
            options ??= new ShareFileOpenWriteOptions() { MaxSize = content.Length };
            var shareClient = serviceClient.GetShareClient(shareName);
            var dirClient = shareClient.GetDirectoryClient(directoryName);
            var fileClient = dirClient.GetFileClient(fileName);
            Stream stream = await fileClient.OpenWriteAsync(true, 0, options, cancellationToken);
            await stream.WriteAsync(content.AsMemory(0, content.Length), cancellationToken);
            await stream.FlushAsync(cancellationToken);
            await stream.DisposeAsync();
            logger?.LogTrace(new EventId(93040, "StorageFile.WriteFileAsync"), $"Directory {directoryName} on file share {shareName} wrote file {fileName} with {content?.Length} bytes.");
        }

        public async Task<byte[]> ReadFileAsync(string shareName, string directoryName, string fileName, ShareFileOpenReadOptions options = null, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            var dirClient = shareClient.GetDirectoryClient(directoryName);
            var fileClient = dirClient.GetFileClient(fileName);
            Stream stream = await fileClient.OpenReadAsync(options, cancellationToken);
            byte[] buffer = new byte[stream.Length];
            _ = await stream.ReadAsync(buffer, cancellationToken);
            await stream.DisposeAsync();
            logger?.LogTrace(new EventId(93050, "StorageFile.ReadFileAsync"), $"Directory {directoryName} on file share {shareName} read file {fileName} with {buffer?.Length} bytes.");
            return buffer;
        }

        public async Task<bool> DeleteFileIfExistsAsync(string shareName, string directoryName, string fileName, ShareFileRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            var dirClient = shareClient.GetDirectoryClient(directoryName);
            var fileClient = dirClient.GetFileClient(fileName);
            Response<bool> response = await fileClient.DeleteIfExistsAsync(conditions, cancellationToken);
            logger?.LogTrace(new EventId(93060, "StorageFile.DeleteFileIfExistsAsync"), $"Directory {directoryName} on file share {shareName} deleted file {fileName} {response.Value}.");
            return response.Value;
        }

        public async Task<List<string>> ListFilesAsync(string shareName, string directoryName, string prefix = null, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            var dirClient = shareClient.GetDirectoryClient(directoryName);
            var result = dirClient.GetFilesAndDirectoriesAsync(prefix, cancellationToken)
                .AsPages(default, null);

            List<string> fileNames = new();
            await foreach (Page<ShareFileItem> shareFilePage in result)
            {
                foreach (ShareFileItem shareFileItem in shareFilePage.Values)
                {
                    if (!shareFileItem.IsDirectory)
                    {
                        fileNames.Add(shareFileItem.Name);
                    }
                }
            }

            logger?.LogTrace(new EventId(93070, "StorageFile.ListFilesAsync"), $"Directory {directoryName} on file share {shareName} listed {fileNames.Count} files.");
            return fileNames;
        }

        public async Task<List<string>> ListDirectoriesAsync(string shareName, string directoryName, string prefix = null, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            var dirClient = shareClient.GetDirectoryClient(directoryName);
            var result = dirClient.GetFilesAndDirectoriesAsync(prefix, cancellationToken)
                .AsPages(default, null);

            List<string> dirNames = new();
            await foreach (Page<ShareFileItem> shareFilePage in result)
            {
                foreach (ShareFileItem shareFileItem in shareFilePage.Values)
                {
                    if (shareFileItem.IsDirectory)
                    {
                        dirNames.Add(shareFileItem.Name);
                    }
                }
            }
            logger?.LogTrace(new EventId(93080, "StorageFile.ListFilesAsync"), $"Directory {directoryName} on file share {shareName} listed {dirNames.Count} directories.");
            return dirNames;
        }

    }
}
