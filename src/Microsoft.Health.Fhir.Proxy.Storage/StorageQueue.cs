using Azure;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Storage
{
    public class StorageQueue
    {
        private readonly QueueServiceClient serviceClient;
        private readonly ILogger logger;

        #region ctor
        public StorageQueue(string connectionString, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new QueueServiceClient(connectionString);
        }
        public StorageQueue(string connectionString, QueueClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new QueueServiceClient(connectionString, options);
        }

        public StorageQueue(Uri serviceUri, QueueClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new QueueServiceClient(serviceUri, options);
        }

        public StorageQueue(Uri serviceUri, TokenCredential credential, QueueClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new QueueServiceClient(serviceUri, credential, options);
        }

        public StorageQueue(Uri serviceUri, AzureSasCredential credential, QueueClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new QueueServiceClient(serviceUri, credential, options);
        }

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

        public async Task<bool> CreateQueueIfNotExistsAsync(string queueName, IDictionary<string, string> metadata = null, CancellationToken cancellationToken = default)
        {
            QueueClient queueClient = serviceClient.GetQueueClient(queueName);
            Response response = await queueClient.CreateIfNotExistsAsync(metadata, cancellationToken);
            bool result = response?.Status != null;
            logger?.LogTrace(new EventId(96010, "StorageQueue.CreateQueueIfNotExistsAsync"), $"Created queue {queueName} with status code {result}.");
            return result;
        }

        public async Task<bool> DeleteQueueIfExistsAsync(string queueName, CancellationToken cancellationToken = default)
        {
            QueueClient queueClient = serviceClient.GetQueueClient(queueName);
            Response<bool> response = await queueClient.DeleteIfExistsAsync(cancellationToken);
            logger?.LogTrace(new EventId(96020, "StorageQueue.DeleteQueueIfExistsAsync"), $"Delete queue {queueName} {response.Value}.");
            return response.Value;
        }

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

            logger?.LogTrace(new EventId(96030, "StorageQueue.ListQueuesAync"), $"Returned list of {queueNames.Count} queue names.");
            return queueNames;
        }

        public async Task<SendReceipt> EnqueueAsync(string queueName, byte[] message, TimeSpan? visibilityTimeout, TimeSpan? ttl, CancellationToken cancellationToken = default)
        {
            BinaryData data = new(message);
            QueueClient client = serviceClient.GetQueueClient(queueName);
            var response = await client.SendMessageAsync(data, visibilityTimeout, ttl, cancellationToken);
            logger?.LogTrace(new EventId(96040, "StorageQueue.EnqueueAsync"), $"Enqueued message in {queueName} queue.");
            return response.Value;
        }

        public async Task<SendReceipt> EnqueueAsync(string queueName, string message, TimeSpan? visibilityTimeout, TimeSpan? ttl, CancellationToken cancellationToken = default)
        {
            QueueClient client = serviceClient.GetQueueClient(queueName.ToLowerInvariant());
            var response = await client.SendMessageAsync(message, visibilityTimeout, ttl, cancellationToken);
            logger?.LogTrace(new EventId(96050, "StorageQueue.EnqueueAsync"), $"Enqueued message in {queueName} queue.");
            return response.Value;
        }

        public async Task<QueueMessage> DequeueAsync(string queueName, TimeSpan? visibilityTimeout, CancellationToken cancellationToken = default)
        {
            QueueClient client = serviceClient.GetQueueClient(queueName.ToLowerInvariant());
            var response = await client.ReceiveMessageAsync(visibilityTimeout, cancellationToken);
            logger?.LogTrace(new EventId(96060, "StorageQueue.DequeueAsync"), $"Dequeued message in {queueName} queue.");
            return response.Value;
        }

        public async Task<QueueMessage[]> DequeueBatchAsync(string queueName, int? maxMessages, TimeSpan? visibilityTimeout, CancellationToken cancellationToken = default)
        {
            QueueClient client = serviceClient.GetQueueClient(queueName.ToLowerInvariant());
            var response = await client.ReceiveMessagesAsync(maxMessages, visibilityTimeout, cancellationToken);
            logger?.LogTrace(new EventId(96070, "StorageQueue.DequeueBatchAsync"), $"Dequeued batch messages in {queueName} queue.");
            return response.Value;
        }

        public async Task<PeekedMessage> PeekMessageAsync(string queueName, CancellationToken cancellationToken = default)
        {
            QueueClient client = serviceClient.GetQueueClient(queueName.ToLowerInvariant());
            var response = await client.PeekMessageAsync(cancellationToken);
            logger?.LogTrace(new EventId(96080, "StorageQueue.PeekMessageAsync"), $"Peeked message in {queueName} queue.");
            return response.Value;
        }

        public async Task<PeekedMessage[]> PeekMessagesAsync(string queueName, int? maxMessages, CancellationToken cancellationToken = default)
        {
            QueueClient client = serviceClient.GetQueueClient(queueName.ToLowerInvariant());
            var response = await client.PeekMessagesAsync(maxMessages, cancellationToken);
            logger?.LogTrace(new EventId(96090, "StorageQueue.PeekMessagesAsync"), $"Peeked messages in {queueName} queue.");
            return response.Value;
        }

        public async Task ClearMessagesAsync(string queueName, CancellationToken cancellationToken = default)
        {
            QueueClient client = serviceClient.GetQueueClient(queueName.ToLowerInvariant());
            _ = await client.ClearMessagesAsync(cancellationToken);
            logger?.LogTrace(new EventId(96100, "StorageQueue.ClearMessagesAsync"), $"Cleared messages in {queueName} queue.");
        }
    }
}
