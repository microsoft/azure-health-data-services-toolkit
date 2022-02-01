using Fhir.Proxy.Pipelines;

namespace Fhir.Proxy.Extensions.Channels
{
    /// <summary>
    /// Azure Blob Storage channel options used to send data to storage.
    /// </summary>
    public class BlobStorageSendOptions
    {
        /// <summary>
        /// Gets or sets the Azure Blob Storage connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Azure Blob Storage container to read or write data.
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        /// Gets or sets the requirement for execution of the channel.
        /// </summary>
        public StatusType ExecutionStatusType { get; set; }

        /// <summary>
        /// Gets or sets the size of the first range request in bytes. Blobs smaller than this limit will be downloaded in a single request. Blobs larger than this limit will continue being downloaded in chunks of size MaximumTransferSize.
        /// </summary>
        public long? InitialTransferSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of workers that may be used in a parallel transfer.
        /// </summary>
        public int? MaxConcurrency { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of an transfer in bytes.
        /// </summary>
        public int? MaxTransferSize { get; set; }
    }
}
