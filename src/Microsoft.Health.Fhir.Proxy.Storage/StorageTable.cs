using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Storage
{
    public class StorageTable
    {
        private readonly CloudTableClient tableClient;
        private readonly ILogger logger;

        public StorageTable(string connectionString, ILogger logger = null)
            : this(CloudStorageAccount.Parse(connectionString).TableStorageUri, CloudStorageAccount.Parse(connectionString).Credentials, logger)
        {
        }

        public StorageTable(string connectionString, TableClientConfiguration config = null, ILogger logger = null)
            : this(CloudStorageAccount.Parse(connectionString).TableStorageUri, CloudStorageAccount.Parse(connectionString).Credentials, config, logger)
        {
        }

        public StorageTable(StorageUri storageUri, StorageCredentials credentials, ILogger logger = null)
            : this(logger)
        {
            tableClient = new CloudTableClient(storageUri, credentials);
        }

        public StorageTable(StorageUri storageUri, StorageCredentials credentials, TableClientConfiguration config = null, ILogger logger = null)
            : this(logger)
        {
            tableClient = new CloudTableClient(storageUri, credentials, config);
        }

        public StorageTable(Uri baseUri, StorageCredentials credentials, TableClientConfiguration config = null, ILogger logger = null)
            : this(logger)
        {
            tableClient = new CloudTableClient(baseUri, credentials, config);
        }

        protected StorageTable(ILogger logger = null)
        {
            this.logger = logger;
        }

        public IEnumerable<CloudTable> ListTables()
        {
            IEnumerable<CloudTable> tables = tableClient.ListTables();
            logger?.LogTrace(new EventId(95010, "StorageTable.ListTables"), $"List of {tables.Count()} tables returned.");
            return tables;
        }

        public async Task<TableResultSegment> ListTablesAsync(string prefix = null, int? maxResults = null, TableContinuationToken token = null, CancellationToken cancellationToken = default)
        {
            TableResultSegment segment = await tableClient.ListTablesSegmentedAsync(prefix, maxResults, token, cancellationToken);
            logger?.LogTrace(new EventId(95020, "StorageTable.ListTablesAsync"), $"Table segment of {segment.Count()} tables returned.");
            return segment;
        }

        public async Task<bool> CreateTableIsNotExistsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            bool result = await table.CreateIfNotExistsAsync(cancellationToken);
            logger?.LogTrace(new EventId(95030, "StorageTable.CreateTableAsync"), $"Table {tableName} created if not exists.");
            return result;
        }

        public async Task<bool> DeleteTableIfExistsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            bool result = await table.DeleteIfExistsAsync(cancellationToken);
            logger?.LogTrace(new EventId(95040, "StorageTable.DeleteTableAsync"), $"Table {tableName} deleted if not exists.");
            return result;
        }

        public async Task<TableResult> InsertEntityAsync(string tableName, TableEntity entity, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Insert(entity);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95050, "StorageTable.InsertEntityAsync"), $"Table entity with partition key {entity.PartitionKey} inserted into table {tableName}");
            return result;
        }

        public async Task<TableResult> InsertOrMergeEntityAsync(string tableName, TableEntity entity, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.InsertOrMerge(entity);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95060, "StorageTable.InsertOrMergeEntityAsync"), $"Table entity with partition key {entity.PartitionKey} inserted or merged into table {tableName}");
            return result;
        }

        public async Task<TableResult> InsertOrReplaceEntityAsync(string tableName, TableEntity entity, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.InsertOrReplace(entity);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95070, "StorageTable.InsertOrReplaceEntityAsync"), $"Table entity with partition key {entity.PartitionKey} inserted or replaced into table {tableName}");
            return result;
        }

        public async Task<TableResult> MergeEntityAsync(string tableName, TableEntity entity, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Merge(entity);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95080, "StorageTable.MergeEntityAsync"), $"Table entity with partition key {entity.PartitionKey} merged into table {tableName}");
            return result;
        }

        public async Task<TableResult> DeleteEntityAsync(string tableName, TableEntity entity, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Delete(entity);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95090, "StorageTable.DeleteEntityAsync"), $"Table entity with partition key {entity.PartitionKey} deleted from table {tableName}");
            return result;
        }

        public async Task<TableResult> ReplaceEntityAsync(string tableName, TableEntity entity, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Replace(entity);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95100, "StorageTable.ReplaceEntityAsync"), $"Table entity with partition key {entity.PartitionKey} replaced from table {tableName}");
            return result;
        }

        public async Task<TableResult> RetrieveEntityAsync(string tableName, string partitionKey, string rowKey, List<string> columns = null, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Retrieve(partitionKey, rowKey, columns);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95110, "StorageTable.RetrieveEntityAsync"), $"Table entity with partition key {partitionKey} retrieved from table {tableName}");
            return result;
        }


        public async Task<TableResult> RetrieveEntity<T>(string tableName, string partitionKey, string rowKey, EntityResolver<T> resolver, List<string> columns = null, CancellationToken cancellationToken = default)
            where T : ITableEntity, new()
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Retrieve(partitionKey, rowKey, resolver, columns);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95120, "StorageTable.RetrieveEntity<T>"), $"Table entity with partition key {partitionKey} replaced from table {tableName}");
            return result;
        }

        public async Task<TableQuerySegment<T>> QueryTableAsync<T>(string tableName, string partitionKey, string rowKey, TableContinuationToken token = null, CancellationToken cancellationToken = default)
            where T : ITableEntity, new()
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableQuery<T> query;
            if (!string.IsNullOrEmpty(partitionKey) && !string.IsNullOrEmpty(rowKey))
            {
                string q1 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
                string q2 = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);
                query = new TableQuery<T>().Where(TableQuery.CombineFilters(q1, TableOperators.And, q2));
            }
            else if (!string.IsNullOrEmpty(partitionKey) && string.IsNullOrEmpty(rowKey))
            {
                query = new TableQuery<T>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            }
            else if (string.IsNullOrEmpty(partitionKey) && !string.IsNullOrEmpty(rowKey))
            {
                query = new TableQuery<T>().Where(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));
            }
            else
            {
                query = new TableQuery<T>();
            }

            TableQuerySegment<T> segment = await table.ExecuteQuerySegmentedAsync(query, token, cancellationToken);
            logger?.LogTrace(new EventId(95130, "StorageTable.QueryTable<T>"), $"Table entity with partition key {partitionKey} replaced from table {tableName}");
            return segment;
        }

        public async Task<TableQuerySegment<T>> QueryTableAsync<T>(string tableName, TableQuery<T> query, TableContinuationToken token = null, CancellationToken cancellationToken = default)
            where T : ITableEntity, new()
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableQuerySegment<T> segment = await table.ExecuteQuerySegmentedAsync<T>(query, token, cancellationToken);
            logger?.LogTrace(new EventId(95140, "StorageTable.QueryTable<T>"), $"Table entity queried from {tableName}");
            return segment;
        }
    }
}
