using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace Microsoft.Health.Fhir.Proxy.Storage
{
    /// <summary>
    /// Allows simple access to Azure storage tables.
    /// </summary>
    public class StorageTable
    {
        private readonly CloudTableClient tableClient;
        private readonly ILogger logger;

        /// <summary>
        /// Creates an instance of StorageTable.
        /// </summary>
        /// <param name="connectionString">A connection string includes the authentication information required for your application to access data in an Azure Storage account at runtime.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageTable(string connectionString, ILogger logger = null)
            : this(CloudStorageAccount.Parse(connectionString).TableStorageUri, CloudStorageAccount.Parse(connectionString).Credentials, logger)
        {
        }

        /// <summary>
        /// Creates an instance of StorageTable.
        /// </summary>
        /// <param name="connectionString">A connection string includes the authentication information required for your application to access data in an Azure Storage account at runtime.</param>
        /// <param name="config">The configuration for the table client. If none is passed, the default is used depending on service type.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageTable(string connectionString, TableClientConfiguration config = null, ILogger logger = null)
            : this(CloudStorageAccount.Parse(connectionString).TableStorageUri, CloudStorageAccount.Parse(connectionString).Credentials, config, logger)
        {
        }

        /// <summary>
        /// Creates an instance of StorageTable.
        /// </summary>
        /// <param name="storageUri">A StorageUri object containing the Table service endpoint to use to create the client.</param>
        /// <param name="credentials">A StorageCredentials object.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageTable(StorageUri storageUri, StorageCredentials credentials, ILogger logger = null)
            : this(logger)
        {
            tableClient = new CloudTableClient(storageUri, credentials);
        }

        /// <summary>
        /// Creates an instance of StorageTable.
        /// </summary>
        /// <param name="storageUri">A StorageUri object containing the Table service endpoint to use to create the client.</param>
        /// <param name="credentials">A StorageCredentials object.</param>
        /// <param name="config">The configuration for the table client. If none is passed, the default is used depending on service type.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageTable(StorageUri storageUri, StorageCredentials credentials, TableClientConfiguration config = null, ILogger logger = null)
            : this(logger)
        {
            tableClient = new CloudTableClient(storageUri, credentials, config);
        }

        /// <summary>
        /// Creates an instance of StorageTable.
        /// </summary>
        /// <param name="baseUri">A Uri object containing the Table service endpoint to use to create the client.</param>
        /// <param name="credentials">A StorageCredentials object.</param>
        /// <param name="config">The configuration for the table client. If none is passed, the default is used depending on service type.</param>
        /// <param name="logger">Optional ILogger.</param>
        public StorageTable(Uri baseUri, StorageCredentials credentials, TableClientConfiguration config = null, ILogger logger = null)
            : this(logger)
        {
            tableClient = new CloudTableClient(baseUri, credentials, config);
        }

        protected StorageTable(ILogger logger = null)
        {
            this.logger = logger;
        }


        /// <summary>
        /// Gets a list of tables from table storage.
        /// </summary>
        /// <returns>Enumerable CloudTable</returns>
        public IEnumerable<CloudTable> ListTables()
        {
            IEnumerable<CloudTable> tables = tableClient.ListTables();
            logger?.LogTrace(
                new EventId(95010, "StorageTable.ListTables"),
                message: "List of {count} tables returned.", tables.Count());
            return tables;
        }

        /// <summary>
        /// Gets a list of pageable table in table storage.
        /// </summary>
        /// <param name="prefix">A string containing the table name prefix.</param>
        /// <param name="maxResults">A non-negative integer value that indicates the maximum number of results to be returned at a time, up to the per-operation limit of 5000. If this value is null, the maximum possible number of results will be returned, up to 5000.</param>
        /// <param name="token">A TableContinuationToken returned by a previous listing operation.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>TableResultSegment</returns>
        public async Task<TableResultSegment> ListTablesAsync(string prefix = null, int? maxResults = null, TableContinuationToken token = null, CancellationToken cancellationToken = default)
        {
            TableResultSegment segment = await tableClient.ListTablesSegmentedAsync(prefix, maxResults, token, cancellationToken);
            logger?.LogTrace(new EventId(95020, "StorageTable.ListTablesAsync"), "Table segment of {count} tables returned.", segment.Count());
            return segment;
        }

        /// <summary>
        /// Creates a table if it does not already exist.
        /// </summary>
        /// <param name="tableName">Name of table to create.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if table created or exists; otherwise false.</returns>
        public async Task<bool> CreateTableIsNotExistsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            bool result = await table.CreateIfNotExistsAsync(cancellationToken);
            logger?.LogTrace(new EventId(95030, "StorageTable.CreateTableAsync"), "Table {name} created if not exists.", tableName);
            return result;
        }

        /// <summary>
        /// Deletes a table if already exists.
        /// </summary>
        /// <param name="tableName">Name of table to delete.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if table deleted or not exists; otherwise false.</returns>
        public async Task<bool> DeleteTableIfExistsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            bool result = await table.DeleteIfExistsAsync(cancellationToken);
            logger?.LogTrace(new EventId(95040, "StorageTable.DeleteTableAsync"), "Table {name} deleted if not exists.", tableName);
            return result;
        }

        /// <summary>
        /// Inserts a new entity into a table.
        /// </summary>
        /// <param name="tableName">Name of table to insert entity.</param>
        /// <param name="entity">Entity to insert.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>TableResult</returns>
        public async Task<TableResult> InsertEntityAsync(string tableName, TableEntity entity, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Insert(entity);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95050, "StorageTable.InsertEntityAsync"), "Table entity with partition key {key} inserted into table {name}.", entity.PartitionKey, tableName);
            return result;
        }

        /// <summary>
        /// Creates a new table operation that inserts the given entity into a table if the entity does not exist; if the entity does exist then its contents are merged with the provided entity.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="entity">Entity to insert or merge.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>TableResult</returns>
        public async Task<TableResult> InsertOrMergeEntityAsync(string tableName, TableEntity entity, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.InsertOrMerge(entity);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95060, "StorageTable.InsertOrMergeEntityAsync"), "Table entity with partition key {key} inserted or merged into table {name}", entity.PartitionKey, tableName);
            return result;
        }

        /// <summary>
        /// Creates a new table operation that inserts the given entity into a table if the entity does not exist; if the entity does exist then its contents are replaced with the provided entity.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="entity">Entity to insert or replace.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>TableResult</returns>
        public async Task<TableResult> InsertOrReplaceEntityAsync(string tableName, TableEntity entity, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.InsertOrReplace(entity);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95070, "StorageTable.InsertOrReplaceEntityAsync"), message: "Table entity with partition key {key} inserted or replaced into table {name}.", entity.PartitionKey, tableName);
            return result;
        }

        /// <summary>
        /// Creates a new table operation that merges the contents of the given entity with the existing entity in a table.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="entity">Entity to merage.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>TableResult</returns>
        public async Task<TableResult> MergeEntityAsync(string tableName, TableEntity entity, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Merge(entity);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95080, "StorageTable.MergeEntityAsync"), "Table entity with partition key {key} merged into table {name}.", entity.PartitionKey, tableName);
            return result;
        }

        /// <summary>
        /// Creates a new table operation that deletes the given entity from a table.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="entity">Entity to delete.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>TableResult</returns>
        public async Task<TableResult> DeleteEntityAsync(string tableName, TableEntity entity, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Delete(entity);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95090, "StorageTable.DeleteEntityAsync"), "Table entity with partition key {key} deleted from table {name}.", entity.PartitionKey, tableName);
            return result;
        }

        /// <summary>
        /// Creates a new table operation that replaces the contents of the given entity in a table.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="entity">Entity that will replace the current entity.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>TableResult</returns>
        public async Task<TableResult> ReplaceEntityAsync(string tableName, TableEntity entity, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Replace(entity);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95100, "StorageTable.ReplaceEntityAsync"), "Table entity with partition key {key} replaced from table {name}", entity.PartitionKey, tableName);
            return result;
        }

        /// <summary>
        /// Creates a new table operation that retrieves the contents of the given entity in a table.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="partitionKey">A string containing the partition key of the entity to retrieve.</param>
        /// <param name="rowKey">A string containing the row key of the entity to retrieve.</param>
        /// <param name="columns">Optional list of column names for projection.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>TableResult</returns>
        public async Task<TableResult> RetrieveEntityAsync(string tableName, string partitionKey, string rowKey, List<string> columns = null, CancellationToken cancellationToken = default)
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Retrieve(partitionKey, rowKey, columns);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95110, "StorageTable.RetrieveEntityAsync"), "Table entity with partition key {key} retrieved from table {name}.", partitionKey, tableName);
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T">The class of type for the entity to retrieve.</typeparam>
        /// <param name="tableName">Name of table.</param>
        /// <param name="partitionKey">A string containing the partition key of the entity to retrieve.</param>
        /// <param name="rowKey">A string containing the row key of the entity to retrieve.</param>
        /// <param name="resolver">The EntityResolver implementation to project the entity to retrieve as a particular type in the result.</param>
        /// <param name="columns">Optional list of column names for projection.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>TableResult</returns>
        public async Task<TableResult> RetrieveEntity<T>(string tableName, string partitionKey, string rowKey, EntityResolver<T> resolver, List<string> columns = null, CancellationToken cancellationToken = default)
            where T : ITableEntity, new()
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation operation = TableOperation.Retrieve(partitionKey, rowKey, resolver, columns);
            TableResult result = await table.ExecuteAsync(operation, cancellationToken);
            logger?.LogTrace(new EventId(95120, "StorageTable.RetrieveEntity<T>"), "Table entity with partition key {key} replaced from table {name}.", partitionKey, tableName);
            return result;
        }


        /// <summary>
        /// Queries entities from a table based on an equal operation where either/or partition key or row key is matched.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="tableName">Name of table.</param>
        /// <param name="partitionKey">Optional string containing the partition key.</param>
        /// <param name="rowKey">Optional string containing the row key.</param>
        /// <param name="token">Optional TableContinuationToken used when the operation returns a partial result.</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>TableQuerySegment</returns>
        public async Task<TableQuerySegment<T>> QueryTableAsync<T>(string tableName, string partitionKey = null, string rowKey = null, TableContinuationToken token = null, CancellationToken cancellationToken = default)
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
            logger?.LogTrace(new EventId(95130, "StorageTable.QueryTable<T>"), "Table entity with partition key {key} replaced from table {name}.", partitionKey, tableName);
            return segment;
        }

        /// <summary>
        /// Queries entities from a table based on a input table query.
        /// </summary>
        /// <typeparam name="T">The class of type for the entity to retrieve.</typeparam>
        /// <param name="tableName">Name of table.</param>
        /// <param name="query">Query to run on the table.</param>
        /// <param name="token">Optional TableContinuationToken object representing a continuation token from the server when the operation returns a partial result.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>TableQuerySegment&lt;T&&gt;</returns>
        public async Task<TableQuerySegment<T>> QueryTableAsync<T>(string tableName, TableQuery<T> query, TableContinuationToken token = null, CancellationToken cancellationToken = default)
            where T : ITableEntity, new()
        {
            CloudTable table = tableClient.GetTableReference(tableName);
            TableQuerySegment<T> segment = await table.ExecuteQuerySegmentedAsync<T>(query, token, cancellationToken);
            logger?.LogTrace(new EventId(95140, "StorageTable.QueryTable<T>"), "Table entity queried from {name}", tableName);
            return segment;
        }
    }
}
