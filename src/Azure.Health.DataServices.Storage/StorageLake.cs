using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Health.DataServices.Storage
{
    /// <summary>
    /// Allows simple access to Azure Data Lake.
    /// </summary>
    public class StorageLake
    {
        private readonly DataLakeServiceClient serviceClient;
        private readonly ILogger logger;

        #region ctor
        /// <summary>
        /// Creates an instance of StorageLake.
        /// </summary>
        /// <param name="connectionString">A connection string includes the authentication information required for your application to access data in an Azure Storage account at runtime.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageLake(string connectionString, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(connectionString);
        }

        /// <summary>
        /// Creates an instance of StorageLake.
        /// </summary>
        /// <param name="serviceUri">A Uri referencing the Data Lake service</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageLake(Uri serviceUri, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri);
        }

        /// <summary>
        /// Creates an instance of StorageLake.
        /// </summary>
        /// <param name="connectionString">A connection string includes the authentication information required for your application to access data in an Azure Storage account at runtime.</param>
        /// <param name="options">Client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageLake(string connectionString, DataLakeClientOptions options, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(connectionString, options);
        }

        /// <summary>
        /// Creates an instance of StorageLake.
        /// </summary>
        /// <param name="serviceUri">A Uri referencing the Data Lake service</param>
        /// <param name="credential">The token credential used to sign requests.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageLake(Uri serviceUri, TokenCredential credential, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri, credential);
        }

        /// <summary>
        /// Creates an instance of StorageLake.
        /// </summary>
        /// <param name="serviceUri">A Uri referencing the Data Lake service</param>
        /// <param name="credential">The shared access signature credential used to sign requests.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageLake(Uri serviceUri, AzureSasCredential credential, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri, credential);
        }

        /// <summary>
        /// Creates an instance of StorageLake.
        /// </summary>
        /// <param name="serviceUri">A Uri referencing the Data Lake service</param>
        /// <param name="options">Client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageLake(Uri serviceUri, DataLakeClientOptions options, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri, options);
        }

        /// <summary>
        /// Creates an instance of StorageLake.
        /// </summary>
        /// <param name="serviceUri">A Uri referencing the Data Lake service</param>
        /// <param name="credential">The shared key credential used to sign requests.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageLake(Uri serviceUri, StorageSharedKeyCredential credential, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri, credential);
        }

        /// <summary>
        /// Creates an instance of StorageLake.
        /// </summary>
        /// <param name="serviceUri">A Uri referencing the Data Lake service</param>
        /// <param name="credential">The token credential used to sign requests.</param>
        /// <param name="options">Client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageLake(Uri serviceUri, TokenCredential credential, DataLakeClientOptions options, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri, credential, options);
        }

        /// <summary>
        /// Creates an instance of StorageLake.
        /// </summary>
        /// <param name="serviceUri">A Uri referencing the Data Lake service</param>
        /// <param name="credential">The shared access signature credential used to sign requests.</param>
        /// <param name="options">Client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageLake(Uri serviceUri, AzureSasCredential credential, DataLakeClientOptions options, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new DataLakeServiceClient(serviceUri, credential, options);
        }

        /// <summary>
        /// Creates an instance of StorageLake.
        /// </summary>
        /// <param name="serviceUri">A Uri referencing the Data Lake service</param>
        /// <param name="credential">The shared key credential used to sign requests.</param>
        /// <param name="options">Client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional ILogger.</param>
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

        /// <summary>
        /// Creates a new file system in data lake.
        /// </summary>
        /// <param name="fileSystemName">Name of file system to create.</param>
        /// <param name="publicAccess">Optionally specifies whether data in the file system may be accessed publicly and the level of access; defualt in "None", i.e., no public access.</param>
        /// <param name="metadata">Optional file system metadata.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns></returns>
        public async Task CreateFileSystemAsync(string fileSystemName, PublicAccessType publicAccess = PublicAccessType.None, IDictionary<string, string> metadata = null, CancellationToken cancellationToken = default)
        {
            _ = await serviceClient.CreateFileSystemAsync(fileSystemName, publicAccess, metadata, cancellationToken);
            logger?.LogTrace(new EventId(92010, "StorageLake.CreateFileSystemAsync"), "File system {FileSystemName} created.", fileSystemName);
        }

        /// <summary>
        /// Indicates whether a file system exists.
        /// </summary>
        /// <param name="fileSystemName">Name of file system to check for existence.</param>
        /// <returns>True if file system exists; otherwise false.</returns>
        public async Task<bool> FileSystemExistsAsync(string fileSystemName)
        {
            var fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            var response = await fsClient.ExistsAsync();
            logger?.LogTrace(new EventId(92020, "StorageLake.FileSystemExistsAsync"), "File system {FileSystemName} exists {Value}.", fileSystemName, response.Value);
            return response.Value;
        }

        /// <summary>
        /// Deletes an existing file system.
        /// </summary>
        /// <param name="fileSystemName">Name of file system to delete.</param>
        /// <param name="conditions">Optional DataLakeRequestConditions to add conditions on the deletion of this file system.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task</returns>
        public async Task DeleteFileSystemAsnyc(string fileSystemName, DataLakeRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            _ = await serviceClient.DeleteFileSystemAsync(fileSystemName, conditions, cancellationToken);
            logger?.LogTrace(new EventId(92030, "StorageLake.DeleteFileSystemAsnyc"), "File system {FileSystemName} deleted.", fileSystemName);
        }

        /// <summary>
        /// Creates a directory within an existing file system.
        /// </summary>
        /// <param name="fileSystemName">Name of file system where the directory will be created.</param>
        /// <param name="path">Path to directory to create.</param>
        /// <param name="httpHeaders">Optional standard HTTP header properties that can be set for the new file or directory..</param>
        /// <param name="metadata">Optional custom metadata to set for this file or directory.</param>
        /// <param name="permissions">Optional and only valid if Hierarchical Namespace is enabled for the account. Sets POSIX access permissions for the file owner, the file owning group, and others. Each class may be granted read, write, or execute permission. The sticky bit is also supported. Both symbolic (rwxrw-rw-) and 4-digit octal notation (e.g. 0766) are supported.</param>
        /// <param name="unmask">Optional and only valid if Hierarchical Namespace is enabled for the account. When creating a file or directory and the parent folder does not have a default ACL, the umask restricts the permissions of the file or directory to be created. The resulting permission is given by p bitwise-and ^u, where p is the permission and u is the umask. For example, if p is 0777 and u is 0057, then the resulting permission is 0720. The default permission is 0777 for a directory and 0666 for a file. The default umask is 0027. The umask must be specified in 4-digit octal notation (e.g. 0766).</param>
        /// <param name="conditions">Optional DataLakeRequestConditions to add conditions on the creation of this file or directory.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task</returns>
        public async Task CreateDirectoryAsync(string fileSystemName, string path, PathHttpHeaders httpHeaders = null, IDictionary<string, string> metadata = null, string permissions = null, string unmask = null, DataLakeRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            await fsClient.CreateDirectoryAsync(path, httpHeaders, metadata, permissions, unmask, conditions, cancellationToken);
            logger?.LogTrace(new EventId(92040, "StorageLake.CreateDirectoryAsync"), "File system {FileSystemName} created directory {Path}.", fileSystemName, path);
        }

        /// <summary>
        /// Indicates whether a directory exists within a file system.
        /// </summary>
        /// <param name="fileSystemName">Name of file system to check.</param>
        /// <param name="path">Path to directory to check.</param>
        /// <returns>True if directory exists; otherwise false.</returns>
        public async Task<bool> DirectoryExistsAsync(string fileSystemName, string path)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            var dirClient = fsClient.GetDirectoryClient(path);
            Response<bool> response = await dirClient.ExistsAsync();
            logger?.LogTrace(new EventId(92050, "StorageLake.DirectoryExistsAsync"), "File system {FileSystemName} with directory {Path} exists {Value}.", fileSystemName, path, response.Value);
            return response.Value;
        }

        /// <summary>
        /// Deletes a directory from a file system.
        /// </summary>
        /// <param name="fileSystemName">Name of file system that contains the directory to delete.</param>
        /// <param name="path">Path to the directory.</param>
        /// <param name="conditions">Optional DataLakeRequestConditions to add conditions on the creation of this file or directory.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task</returns>
        public async Task DeleteDirectoryAsync(string fileSystemName, string path, DataLakeRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            _ = await fsClient.DeleteDirectoryAsync(path, conditions, cancellationToken);
            logger?.LogTrace(new EventId(92060, "StorageLake.DeleteDirectoryAsync"), "File system {FileSystemName} with directory {Path} deleted.", fileSystemName, path);
        }

        /// <summary>
        /// Uploads a local file to data lake storage.
        /// </summary>
        /// <param name="fileSystemName">Name of file system.</param>
        /// <param name="directoryName">Name of directory in file system as destination for upload.</param>
        /// <param name="filename">File name to create in the directory for the upload.</param>
        /// <param name="content">Content stream to upload.</param>
        /// <param name="options">DataLake file upload options</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task</returns>
        public async Task UploadFileAsync(string fileSystemName, string directoryName, string filename, Stream content, DataLakeFileUploadOptions options, CancellationToken cancellationToken = default)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            DataLakeDirectoryClient directoryClient = fsClient.GetDirectoryClient(directoryName);

            DataLakeFileClient fileClient = await directoryClient.CreateFileAsync(filename, cancellationToken: cancellationToken);
            _ = await fileClient.UploadAsync(content, options, cancellationToken);
            logger?.LogTrace(new EventId(92070, "StorageLake.UploadFileAsync"), "File system {FileSystemName} with directory {DirectoryName} uploaded file {FileName}.", fileSystemName, directoryName, filename);
        }

        /// <summary>
        /// Uploads a local file to data lake storage.
        /// </summary>
        /// <param name="fileSystemName">Name of file system.</param>
        /// <param name="directoryName">Name of directory in file system as destination for upload.</param>
        /// <param name="filename">File name to create in the directory for the upload.</param>
        /// <param name="content">Content array of bytes to upload.</param>
        /// <param name="options">DataLake file open write options.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task</returns>
        public async Task WriteFileAsync(string fileSystemName, string directoryName, string filename, byte[] content, DataLakeFileOpenWriteOptions options = null, CancellationToken cancellationToken = default)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            DataLakeDirectoryClient directoryClient = fsClient.GetDirectoryClient(directoryName);
            DataLakeFileClient fileClient = await directoryClient.CreateFileAsync(filename, cancellationToken: cancellationToken);
            using Stream stream = await fileClient.OpenWriteAsync(true, options, cancellationToken);
            await stream.WriteAsync(content.AsMemory(0, content.Length), cancellationToken);
            await stream.FlushAsync(cancellationToken);
            await stream.DisposeAsync();
            logger?.LogTrace(new EventId(92080, "StorageLake.WriteFileAsync"), "File system {FileSystemName} with directory {DirectoryName} wrote file {FileName}.", fileSystemName, directoryName, filename);
        }

        /// <summary>
        /// Reads a file from data lake storage.
        /// </summary>
        /// <param name="fileSystemName">Name of file system.</param>
        /// <param name="directoryName">Name of directory in file system as the destination read.</param>
        /// <param name="filename">Name of file to read.</param>
        /// <param name="options">Data lake read options.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Array of byte of the content of the file read.</returns>
        public async Task<byte[]> ReadFileAsync(string fileSystemName, string directoryName, string filename, DataLakeOpenReadOptions options = null, CancellationToken cancellationToken = default)
        {
            options ??= new DataLakeOpenReadOptions(true);
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            DataLakeDirectoryClient directoryClient = fsClient.GetDirectoryClient(directoryName);
            DataLakeFileClient fileClient = directoryClient.GetFileClient(filename);
            Stream stream = await fileClient.OpenReadAsync(options, cancellationToken);
            byte[] buffer = new byte[stream.Length];
            _ = await stream.ReadAsync(buffer, cancellationToken);
            await stream.DisposeAsync();
            logger?.LogTrace(new EventId(92090, "StorageLake.ReadFileAsync"), "File system {FileSystemName} with directory {DirectoryName} read file {FileName} with {Count} bytes.", fileSystemName, directoryName, filename, buffer?.Length);
            return buffer;
        }

        /// <summary>
        /// Uploads content to a file in data lake.
        /// </summary>
        /// <param name="fileSystemName">Name of file system.</param>
        /// <param name="directoryName">Name of directory in file system to write.</param>
        /// <param name="filename">Name of file to write.</param>
        /// <param name="overwrite">Indicators whether the file should be overwritten if exists.</param>
        /// <param name="content">File content as stream.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task</returns>
        public async Task UploadFileAsync(string fileSystemName, string directoryName, string filename, bool overwrite, Stream content, CancellationToken cancellationToken = default)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            DataLakeDirectoryClient directoryClient = fsClient.GetDirectoryClient(directoryName);
            DataLakeFileClient fileClient = await directoryClient.CreateFileAsync(filename, cancellationToken: cancellationToken);
            _ = await fileClient.UploadAsync(content, overwrite, cancellationToken);
            logger?.LogTrace(new EventId(92100, "StorageLake.UploadFileAsync"), "File system {FileSystemName} with directory {DirectoryName} uploaded file {FileName}.", fileSystemName, directoryName, filename);
        }

        /// <summary>
        /// Deletes a file from data lake.
        /// </summary>
        /// <param name="fileSystemName">Name of file system.</param>
        /// <param name="directoryName">Name of directory in file system.</param>
        /// <param name="filename">Name of file to delete.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if file is deleted; otherwise false.</returns>
        public async Task<bool> DeleteFileAsync(string fileSystemName, string directoryName, string filename, CancellationToken cancellationToken = default)
        {
            DataLakeFileSystemClient fsClient = serviceClient.GetFileSystemClient(fileSystemName);
            DataLakeDirectoryClient directoryClient = fsClient.GetDirectoryClient(directoryName);
            DataLakeFileClient fileClient = directoryClient.GetFileClient(filename);
            Response<bool> response = await fileClient.DeleteIfExistsAsync(null, cancellationToken);
            logger?.LogTrace(new EventId(92110, "StorageLake.UploadFileAsync"), "File system {FileSystemName} with directory {DirectoryName} deleted file {FileName} {Value}.", fileSystemName, directoryName, filename, response.Value);
            return response.Value;
        }
    }
}
