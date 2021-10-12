using Azure;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Storage
{
    public class StorageLake
    {
        private readonly DataLakeServiceClient serviceClient;
        private readonly ILogger logger;

        #region ctor
        public StorageLake(string connectionString, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(connectionString);
        }

        public StorageLake(Uri serviceUri, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri);
        }

        public StorageLake(string connectionString, DataLakeClientOptions options, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(connectionString, options);
        }

        public StorageLake(Uri serviceUri, TokenCredential credential, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri, credential);
        }

        public StorageLake(Uri serviceUri, AzureSasCredential credential, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri, credential);
        }

        public StorageLake(Uri serviceUri, DataLakeClientOptions options, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri, options);
        }

        public StorageLake(Uri serviceUri, StorageSharedKeyCredential credential, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri, credential);
        }

        public StorageLake(Uri serviceUri, TokenCredential credential, DataLakeClientOptions options, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri, credential, options);
        }

        public StorageLake(Uri serviceUri, AzureSasCredential credential, DataLakeClientOptions options, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri, credential, options);
        }

        public StorageLake(Uri serviceUri, StorageSharedKeyCredential credential, DataLakeClientOptions options, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri, credential, options);
        }

        protected StorageLake(ILogger logger = null)
        {
            this.logger = logger;
        }

        #endregion

        public async Task CreateFileSystemAsync(string fileSystemName, PublicAccessType publicAccess = PublicAccessType.None, IDictionary<string, string> metadata = null, CancellationToken cancellationToken = default)
        {
            _ = await serviceClient.CreateFileSystemAsync(fileSystemName, publicAccess, metadata, cancellationToken);
            logger?.LogTrace(new EventId(92010, "StorageLake.CreateFileSystemAsync"), $"File system {fileSystemName} created.");
        }

        public async Task<bool> FileSystemExistsAsync(string fileSystemName)
        {
            var fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            var response = await fsClient.ExistsAsync();
            logger?.LogTrace(new EventId(92020, "StorageLake.FileSystemExistsAsync"), $"File system {fileSystemName} exists {response.Value}.");
            return response.Value;
        }

        public async Task DeleteFileSystemAsnyc(string fileSystemName, DataLakeRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            _ = await serviceClient.DeleteFileSystemAsync(fileSystemName, conditions, cancellationToken);
            logger?.LogTrace(new EventId(92030, "StorageLake.DeleteFileSystemAsnyc"), $"File system {fileSystemName} deleted.");
        }

        public async Task CreateDirectoryAsync(string fileSystemName, string path, PathHttpHeaders httpHeaders = null, IDictionary<string, string> metadata = null, string permissions = null, string unmask = null, DataLakeRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            await fsClient.CreateDirectoryAsync(path, httpHeaders, metadata, permissions, unmask, conditions, cancellationToken);
            logger?.LogTrace(new EventId(92040, "StorageLake.CreateDirectoryAsync"), $"File system {fileSystemName} created directory {path}.");
        }

        public async Task<bool> DirectoryExistsAsync(string fileSystemName, string path)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            var dirClient = fsClient.GetDirectoryClient(path);
            Response<bool> response = await dirClient.ExistsAsync();
            logger?.LogTrace(new EventId(92050, "StorageLake.DirectoryExistsAsync"), $"File system {fileSystemName} with directory {path} exists {response.Value}.");
            return response.Value;
        }

        public async Task DeleteDirectoryAsync(string fileSystemName, string path, DataLakeRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            _ = await fsClient.DeleteDirectoryAsync(path, conditions, cancellationToken);
            logger?.LogTrace(new EventId(92060, "StorageLake.DeleteDirectoryAsync"), $"File system {fileSystemName} with directory {path} deleted.");
        }

        public async Task UploadFileAsync(string fileSystemName, string path, string filename, Stream content, DataLakeFileUploadOptions options, CancellationToken cancellationToken = default)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            DataLakeDirectoryClient directoryClient = fsClient.GetDirectoryClient(path);

            DataLakeFileClient fileClient = await directoryClient.CreateFileAsync(filename, cancellationToken: cancellationToken);
            _ = await fileClient.UploadAsync(content, options, cancellationToken);
            logger?.LogTrace(new EventId(92070, "StorageLake.UploadFileAsync"), $"File system {fileSystemName} with directory {path} uploaded file {filename}.");
        }

        public async Task WriteFileAsync(string fileSystemName, string path, string filename, byte[] content, DataLakeFileOpenWriteOptions options = null, CancellationToken cancellationToken = default)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            DataLakeDirectoryClient directoryClient = fsClient.GetDirectoryClient(path);
            DataLakeFileClient fileClient = await directoryClient.CreateFileAsync(filename, cancellationToken: cancellationToken);
            using Stream stream = await fileClient.OpenWriteAsync(true, options, cancellationToken);
            await stream.WriteAsync(content.AsMemory(0, content.Length), cancellationToken);
            await stream.FlushAsync(cancellationToken);
            await stream.DisposeAsync();
            logger?.LogTrace(new EventId(92080, "StorageLake.WriteFileAsync"), $"File system {fileSystemName} with directory {path} wrote file {filename}.");
        }

        public async Task<byte[]> ReadFileAsync(string fileSystemName, string path, string filename, DataLakeOpenReadOptions options = null, CancellationToken cancellationToken = default)
        {
            options ??= new DataLakeOpenReadOptions(true);
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            DataLakeDirectoryClient directoryClient = fsClient.GetDirectoryClient(path);
            DataLakeFileClient fileClient = directoryClient.GetFileClient(filename);
            Stream stream = await fileClient.OpenReadAsync(options, cancellationToken);
            byte[] buffer = new byte[stream.Length];
            _ = await stream.ReadAsync(buffer, cancellationToken);
            await stream.DisposeAsync();
            logger?.LogTrace(new EventId(92090, "StorageLake.ReadFileAsync"), $"File system {fileSystemName} with directory {path} read file {filename} with {buffer?.Length} bytes.");
            return buffer;
        }

        public async Task UploadFileAsync(string fileSystemName, string path, string filename, bool overwrite, Stream content, CancellationToken cancellationToken = default)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            DataLakeDirectoryClient directoryClient = fsClient.GetDirectoryClient(path);
            DataLakeFileClient fileClient = await directoryClient.CreateFileAsync(filename, cancellationToken: cancellationToken);
            _ = await fileClient.UploadAsync(content, overwrite, cancellationToken);
            logger?.LogTrace(new EventId(92100, "StorageLake.UploadFileAsync"), $"File system {fileSystemName} with directory {path} uploaded file {filename}.");
        }

        public async Task<bool> DeleteFileAsync(string fileSystemName, string path, string filename, CancellationToken cancellationToken = default)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            DataLakeDirectoryClient directoryClient = fsClient.GetDirectoryClient(path);
            DataLakeFileClient fileClient = directoryClient.GetFileClient(filename);
            Response<bool> response = await fileClient.DeleteIfExistsAsync(null, cancellationToken);
            logger?.LogTrace(new EventId(92110, "StorageLake.UploadFileAsync"), $"File system {fileSystemName} with directory {path} deleted file {filename} {response.Value}.");
            return response.Value;
        }
    }
}
