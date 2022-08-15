using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Health.DataServices.Storage
{
    /// <summary>
    /// Allows simple access to Azure storage queues.
    /// </summary>
    public class StorageQueue
    {
        private readonly QueueServiceClient serviceClient;
        private readonly ILogger logger;

        #region ctor
        /// <summary>
        /// Creates an instance of StorageQueue.
        /// </summary>
        /// <param name="connectionString">A connection string includes the authentication information required for your application to access data in an Azure Storage account at runtime.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageQueue(string connectionString, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new QueueServiceClient(connectionString);
        }

        /// <summary>
        /// Creates an instance of StorageQueue.
        /// </summary>
        /// <param name="connectionString">A connection string includes the authentication information required for your application to access data in an Azure Storage account at runtime.</param>
        /// <param name="options">Optional client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageQueue(string connectionString, QueueClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new QueueServiceClient(connectionString, options);
        }

        /// <summary>
        /// Creates an instance of StorageQueue.
        /// </summary>
        /// <param name="serviceUri">A Uri referencing the queue service.</param>
        /// <param name="options">Optional client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageQueue(Uri serviceUri, QueueClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new QueueServiceClient(serviceUri, options);
        }

        /// <summary>
        /// Creates an instance of StorageQueue.
        /// </summary>
        /// <param name="serviceUri">A Uri referencing the queue service.</param>
        /// <param name="credential">The token credential used to sign requests.</param>
        /// <param name="options">Optional client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageQueue(Uri serviceUri, TokenCredential credential, QueueClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new QueueServiceClient(serviceUri, credential, options);
        }

        /// <summary>
        /// Creates an instance of StorageQueue.
        /// </summary>
        /// <param name="serviceUri">A Uri referencing the queue service.</param>
        /// <param name="credential">The shared access signature credential used to sign requests.</param>
        /// <param name="options">Optional client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageQueue(Uri serviceUri, AzureSasCredential credential, QueueClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new QueueServiceClient(serviceUri, credential, options);
        }

        /// <summary>
        /// Creates an instance of StorageQueue.
        /// </summary>
        /// <param name="serviceUri">A Uri referencing the queue service.</param>
        /// <param name="credential">The shared key credential used to sign requests.</param>
        /// <param name="options">Optional client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageQueue(Uri serviceUri, StorageSharedKeyCredential credential, QueueClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new QueueServiceClient(serviceUri, credential, options);
        }

        protected StorageQueue(ILogger logger = null)
        {
            this.logger = logger;
        }

        #endregion

        /// <summary>
        /// Creates queue if it not does already exist.
        /// </summary>
        /// <param name="queueName">Nmme of queue to create.</param>
        /// <param name="metadata">Optional queue metadata.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if queue created or exists; otherwise false.</returns>
        public async Task<bool> CreateQueueIfNotExistsAsync(string queueName, IDictionary<string, string> metadata = null, CancellationToken cancellationToken = default)
        {
            QueueClient queueClient = serviceClient.GetQueueClient(queueName);
            Response response = await queueClient.CreateIfNotExistsAsync(metadata, cancellationToken);
            bool result = response?.Status != null;
            logger?.LogTrace(new EventId(96010, "StorageQueue.CreateQueueIfNotExistsAsync"), "Created queue {QueueName} with status code {Result}.", queueName, result);
            return result;
        }

        /// <summary>
        /// Deletes a queue if it exists.
        /// </summary>
        /// <param name="queueName">Name of queue to delete.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if queue deleted or not exists; otherwise false.</returns>
        public async Task<bool> DeleteQueueIfExistsAsync(string queueName, CancellationToken cancellationToken = default)
        {
            QueueClient queueClient = serviceClient.GetQueueClient(queueName);
            Response<bool> response = await queueClient.DeleteIfExistsAsync(cancellationToken);
            logger?.LogTrace(new EventId(96020, "StorageQueue.DeleteQueueIfExistsAsync"), "Delete queue {QueueName} {Value}.", queueName, response.Value);
            return response.Value;
        }

        /// <summary>
        /// Gets a list of queue names.
        /// </summary>
        /// <param name="traits">Optional trait options for shaping the queues; default is "None".</param>
        /// <param name="prefix">Optional string that filters the results to return only queues whose name begins with the specified prefix.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Queue names as list of string.</returns>
        public async Task<List<string>> ListQueuesAync(QueueTraits traits = QueueTraits.None, string prefix = null, CancellationToken cancellationToken = default)
        {
            var result = serviceClient.GetQueuesAsync(traits, prefix, cancellationToken)
                .AsPages(default, null);

            List<string> queueNames = new();
            await foreach (Page<QueueItem> queueItem in result)
            {
                foreach (var item in queueItem.Values)
                {
                    queueNames.Add(item.Name);
                }
            }

            logger?.LogTrace(new EventId(96030, "StorageQueue.ListQueuesAync"), message: "Returned list of {Count} queue names.", queueNames.Count);
            return queueNames;
        }

        /// <summary>
        /// Enqueues a message into a queue.
        /// </summary>
        /// <param name="queueName">Name of queue to enqueue message.</param>
        /// <param name="message">Message to enqueue.</param>
        /// <param name="visibilityTimeout">Visibility timeout. Optional with a default value of 0. Cannot be larger than 7 days.</param>
        /// <param name="ttl">Optional. Specifies the time-to-live interval for the message.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>SendReceipt</returns>
        public async Task<SendReceipt> EnqueueAsync(string queueName, byte[] message, TimeSpan? visibilityTimeout, TimeSpan? ttl, CancellationToken cancellationToken = default)
        {
            BinaryData data = new(message);
            QueueClient client = serviceClient.GetQueueClient(queueName);
            var response = await client.SendMessageAsync(data, visibilityTimeout, ttl, cancellationToken);
            logger?.LogTrace(new EventId(96040, "StorageQueue.EnqueueAsync"), message: "Enqueued message in {QueueName} queue.", queueName);
            return response.Value;
        }

        /// <summary>
        /// Enqueues a message into a queue.
        /// </summary>
        /// <param name="queueName">Name of queue to enqueue message.</param>
        /// <param name="message">Message to enqueue.</param>
        /// <param name="visibilityTimeout">Visibility timeout. Optional with a default value of 0. Cannot be larger than 7 days.</param>
        /// <param name="ttl">Optional. Specifies the time-to-live interval for the message.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>SendReceipt</returns>
        public async Task<SendReceipt> EnqueueAsync(string queueName, string message, TimeSpan? visibilityTimeout, TimeSpan? ttl, CancellationToken cancellationToken = default)
        {
            QueueClient client = serviceClient.GetQueueClient(queueName.ToLowerInvariant());
            var response = await client.SendMessageAsync(message, visibilityTimeout, ttl, cancellationToken);
            logger?.LogTrace(new EventId(96050, "StorageQueue.EnqueueAsync"), message: "Enqueued message in {QueueName} queue.", queueName);
            return response.Value;
        }

        /// <summary>
        /// Dequeues a message from a queue.
        /// </summary>
        /// <param name="queueName">Name of queue to dequeue message.</param>
        /// <param name="visibilityTimeout">Visibility timeout. Optional with a default value of 0. Cannot be larger than 7 days.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>QueueMessage</returns>
        public async Task<QueueMessage> DequeueAsync(string queueName, TimeSpan? visibilityTimeout, CancellationToken cancellationToken = default)
        {
            QueueClient client = serviceClient.GetQueueClient(queueName.ToLowerInvariant());
            var response = await client.ReceiveMessageAsync(visibilityTimeout, cancellationToken);
            logger?.LogTrace(new EventId(96060, "StorageQueue.DequeueAsync"), message: "Dequeued message in {QueueName} queue.", queueName);
            return response.Value;
        }

        /// <summary>
        /// Dequeues a batch of message from a queue.
        /// </summary>
        /// <param name="queueName">Name of queue to dequeue message.</param>
        /// <param name="maxMessages">Maximum number of messages to dequeue in batch.</param>
        /// <param name="visibilityTimeout">Visibility timeout. Optional with a default value of 0. Cannot be larger than 7 days.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns></returns>
        public async Task<QueueMessage[]> DequeueBatchAsync(string queueName, int? maxMessages, TimeSpan? visibilityTimeout, CancellationToken cancellationToken = default)
        {
            QueueClient client = serviceClient.GetQueueClient(queueName.ToLowerInvariant());
            var response = await client.ReceiveMessagesAsync(maxMessages, visibilityTimeout, cancellationToken);
            logger?.LogTrace(new EventId(96070, "StorageQueue.DequeueBatchAsync"), message: "Dequeued batch messages in {QueueName} queue.", queueName);
            return response.Value;
        }

        /// <summary>
        /// Peeks a message from a queue.
        /// </summary>
        /// <param name="queueName">Name of queue to peek message.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>PeekedMessage</returns>
        public async Task<PeekedMessage> PeekMessageAsync(string queueName, CancellationToken cancellationToken = default)
        {
            QueueClient client = serviceClient.GetQueueClient(queueName.ToLowerInvariant());
            var response = await client.PeekMessageAsync(cancellationToken);
            logger?.LogTrace(new EventId(96080, "StorageQueue.PeekMessageAsync"), message: "Peeked message in {QueueName} queue.", queueName);
            return response.Value;
        }

        /// <summary>
        /// Peeks a batch of messages from a queue.
        /// </summary>
        /// <param name="queueName">Name of queue to peek a batch of messages.</param>
        /// <param name="maxMessages">Maximum number of messages to peek in queue.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Array of PeekedMessage.</returns>
        public async Task<PeekedMessage[]> PeekMessagesAsync(string queueName, int? maxMessages, CancellationToken cancellationToken = default)
        {
            QueueClient client = serviceClient.GetQueueClient(queueName.ToLowerInvariant());
            var response = await client.PeekMessagesAsync(maxMessages, cancellationToken);
            logger?.LogTrace(new EventId(96090, "StorageQueue.PeekMessagesAsync"), "Peeked messages in {QueueName} queue.", queueName);
            return response.Value;
        }

        /// <summary>
        /// Clears messages from a queue.
        /// </summary>
        /// <param name="queueName">Name of queue to clear messages.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task</returns>
        public async Task ClearMessagesAsync(string queueName, CancellationToken cancellationToken = default)
        {
            QueueClient client = serviceClient.GetQueueClient(queueName.ToLowerInvariant());
            _ = await client.ClearMessagesAsync(cancellationToken);
            logger?.LogTrace(new EventId(96100, "StorageQueue.ClearMessagesAsync"), message: "Cleared messages in {QueueName} queue.", queueName);
        }
    }
}
