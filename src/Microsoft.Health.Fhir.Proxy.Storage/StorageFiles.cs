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
    /// <summary>
    /// Allows simple access to Azure File Storage.
    /// </summary>
    public class StorageFiles
    {
        private readonly ShareServiceClient serviceClient;
        private readonly ILogger logger;

        #region ctor
        /// <summary>
        /// Creates an instance of StorageFiles.
        /// </summary>
        /// <param name="connectionString">Azure storage connection string.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageFiles(string connectionString, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new ShareServiceClient(connectionString);
        }

        /// <summary>
        /// Creates an instance of StorageFiles.
        /// </summary>
        /// <param name="connectionString">Azure storage connection string.</param>
        /// <param name="options">Client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageFiles(string connectionString, ShareClientOptions options, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new ShareServiceClient(connectionString, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceUri">Uri referencing the file service.</param>
        /// <param name="credentials">The shared key credential used to sign requests.</param>
        /// <param name="options">Client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optionak ILogger.</param>
        public StorageFiles(Uri serviceUri, StorageSharedKeyCredential credentials, ShareClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new ShareServiceClient(serviceUri, credentials, options);
        }

        /// <summary>
        /// Creates an instance of StorageFiles.
        /// </summary>
        /// <param name="serviceUri">Uri referencing the file service.</param>
        /// <param name="options">Optional client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageFiles(Uri serviceUri, ShareClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new ShareServiceClient(serviceUri, options);
        }

        /// <summary>
        /// Creates an instance of StorageFiles.
        /// </summary>
        /// <param name="serviceUri">Uri referencing the file service.</param>
        /// <param name="credentials">The shared access signature credential used to sign requests.</param>
        /// <param name="options">Optional client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional ILogger.</param>
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

        /// <summary>
        /// Creates a file share is one does not already exist.
        /// </summary>
        /// <param name="shareName">Name of share to create.</param>
        /// <param name="options">Optional share create options.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>ShareInfo</returns>
        public async Task<ShareInfo> CreateShareIfNotExistsAsync(string shareName, ShareCreateOptions options = null, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            Response<ShareInfo> info = await shareClient.CreateIfNotExistsAsync(options, cancellationToken);
            logger?.LogTrace(new EventId(93000, "StorageFile.CreateShareIfNotExistsAsync"), "File share {0} created.", shareName);
            return info?.Value;
        }

        /// <summary>
        /// Deletes an Azure file share if exists.
        /// </summary>
        /// <param name="shareName">Name of share to delete.</param>
        /// <param name="options">Optional share delete options.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if share is deleted; otherwise false.</returns>
        public async Task<bool> DeleteShareIfExistsAsync(string shareName, ShareDeleteOptions options = null, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            bool result = await shareClient.DeleteIfExistsAsync(options, cancellationToken);
            logger?.LogTrace(new EventId(93010, "StorageFile.DeleteShareIfExistsAsync"), "File share {0} deleted {1}.", shareName, result);
            return result;
        }

        /// <summary>
        /// Creates a directory in a share if the directory does not already exist.
        /// </summary>
        /// <param name="shareName">Name of share where directory should be created.</param>
        /// <param name="directoryName">Name of directory to create.</param>
        /// <param name="metadata">Optional metadata.</param>
        /// <param name="smbProperties">Optional SMB properties.</param>
        /// <param name="filePermission">Optional file permissions.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>ShareDirectoryInfo</returns>
        public async Task<ShareDirectoryInfo> CreateDirectoryIfNotExistsAsync(string shareName, string directoryName, IDictionary<string, string> metadata = null, FileSmbProperties smbProperties = null, string filePermission = null, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            var dirClient = shareClient.GetDirectoryClient(directoryName);
            Response<ShareDirectoryInfo> result = await dirClient.CreateIfNotExistsAsync(metadata, smbProperties, filePermission, cancellationToken);
            logger?.LogTrace(new EventId(93020, "StorageFile.CreateDirectoryIfNotExistsAsync"), "Directory {0} created on file share {1}.", directoryName, shareName);
            return result.Value;
        }

        /// <summary>
        /// Deletes a directory in a share if the directory already exists.
        /// </summary>
        /// <param name="shareName">Name of share where directory exists.</param>
        /// <param name="directoryName">Name of directory to delete.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if the directory is deleted or does not exist; otherwise false.</returns>
        public async Task<bool> DeleteDirectoryIfExistsAsync(string shareName, string directoryName, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            var dirClient = shareClient.GetDirectoryClient(directoryName);
            Response<bool> response = await dirClient.DeleteIfExistsAsync(cancellationToken);
            logger?.LogTrace(new EventId(93030, "StorageFile.DeleteDirectoryIfExistsAsync"), "Directory {0} on file share {1} deleted {2}.", directoryName, shareName, response.Value);
            return response.Value;
        }

        /// <summary>
        /// Writes a file to a directory in a share.
        /// </summary>
        /// <param name="shareName">Name of share where directory exists.</param>
        /// <param name="directoryName">Name of directory to write the file.</param>
        /// <param name="fileName">Name of file to write.</param>
        /// <param name="content">Content of the file to write.</param>
        /// <param name="options">Optional share file open write options.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task</returns>
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
            logger?.LogTrace(new EventId(93040, "StorageFile.WriteFileAsync"), "Directory {0} on file share {1} wrote file {2} with {3} bytes.", directoryName, shareName, fileName, content?.Length);
        }

        /// <summary>
        /// Reads a file from directory in a share and returns the result.
        /// </summary>
        /// <param name="shareName">Name of share where directory exists.</param>
        /// <param name="directoryName">Name of directory where file exists.</param>
        /// <param name="fileName">Name of file to read.</param>
        /// <param name="options">Optional share file open read options.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Array of bytes of the file content.</returns>
        public async Task<byte[]> ReadFileAsync(string shareName, string directoryName, string fileName, ShareFileOpenReadOptions options = null, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            var dirClient = shareClient.GetDirectoryClient(directoryName);
            var fileClient = dirClient.GetFileClient(fileName);
            Stream stream = await fileClient.OpenReadAsync(options, cancellationToken);
            byte[] buffer = new byte[stream.Length];
            _ = await stream.ReadAsync(buffer, cancellationToken);
            await stream.DisposeAsync();
            logger?.LogTrace(new EventId(93050, "StorageFile.ReadFileAsync"), "Directory {0} on file share {1} read file {2} with {3} bytes.", directoryName, shareName, fileName, buffer?.Length);
            return buffer;
        }

        /// <summary>
        /// Deletes a file in directory of a share if it already exists.
        /// </summary>
        /// <param name="shareName">Name of share where directory exists.</param>
        /// <param name="directoryName">Name of directory that contains the file.</param>
        /// <param name="fileName">File name to delete.</param>
        /// <param name="conditions">Optional share file request options.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if the file is deleted or does not exist; otherwise false.</returns>
        public async Task<bool> DeleteFileIfExistsAsync(string shareName, string directoryName, string fileName, ShareFileRequestConditions conditions = null, CancellationToken cancellationToken = default)
        {
            var shareClient = serviceClient.GetShareClient(shareName);
            var dirClient = shareClient.GetDirectoryClient(directoryName);
            var fileClient = dirClient.GetFileClient(fileName);
            Response<bool> response = await fileClient.DeleteIfExistsAsync(conditions, cancellationToken);
            logger?.LogTrace(new EventId(93060, "StorageFile.DeleteFileIfExistsAsync"), "Directory {0} on file share {1} deleted file {2} {3}.", directoryName, shareName, fileName, response.Value);
            return response.Value;
        }

        /// <summary>
        /// Gets a list of file names from a directory in a share.
        /// </summary>
        /// <param name="shareName">Name of share where directory exists.</param>
        /// <param name="directoryName">Name of directory to read list of files.</param>
        /// <param name="prefix">Optional prefix.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>File names in directory as list of strings.</returns>
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

            logger?.LogTrace(new EventId(93070, "StorageFile.ListFilesAsync"), "Directory {0} on file share {1} listed {2} files.", directoryName, shareName, fileNames.Count);
            return fileNames;
        }

        /// <summary>
        /// Gets a list of sub-directories in directory of a share.
        /// </summary>
        /// <param name="shareName">Name of share to read directory names.</param>
        /// <param name="directoryName">Name of parent directory to read names of sub-directories.</param>
        /// <param name="prefix">Optional prefix.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Sub-directory names as a list of strings.</returns>
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
            logger?.LogTrace(new EventId(93080, "StorageFile.ListFilesAsync"), "Directory {0} on file share {1} listed {2} directories.", directoryName, shareName, dirNames.Count);
            return dirNames;
        }

    }
}
