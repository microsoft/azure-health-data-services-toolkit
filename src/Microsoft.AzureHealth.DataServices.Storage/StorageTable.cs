using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureHealth.DataServices.Storage
{
    /// <summary>
    /// Allows simple access to Azure storage tables.
    /// </summary>
    public class StorageTable
    {
        private readonly TableServiceClient serviceClient;
        private readonly ILogger logger;

        /// <summary>
        /// Creates an instance of StorageTable.
        /// </summary>
        /// <param name="connectionString">A connection string includes the authentication information required for your application to access data in an Azure Storage account at runtime.</param>
        /// <param name="logger">Optional lLogger for observability.</param>
        public StorageTable(string connectionString, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new TableServiceClient(connectionString);
        }

        /// <summary>
        /// Creates an instance of StorageTable.
        /// </summary>
        /// <param name="endpoint">A Uri referencing the table service account. </param>
        /// <param name="options">Optional client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional lLogger for observability.</param>
        public StorageTable(Uri endpoint, TableClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new TableServiceClient(endpoint, options);
        }

        /// <summary>
        /// Creates an instance of StorageTable.
        /// </summary>
        /// <param name="connectionString">A connection string includes the authentication information required for your application to access data in an Azure Storage account at runtime.</param>
        /// <param name="options">Optional client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional lLogger for observability.</param>
        public StorageTable(string connectionString, TableClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new TableServiceClient(connectionString, options);
        }

        /// <summary>
        /// Creates an instance of StorageTable.
        /// </summary>
        /// <param name="endpoint">A Uri referencing the table service account. </param>
        /// <param name="credential">The shared access signature credential used to sign requests.</param>
        /// <param name="options">Optional client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional lLogger for observability.</param>
        public StorageTable(Uri endpoint, AzureSasCredential credential, TableClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new TableServiceClient(endpoint, credential, options);
        }

        /// <summary>
        /// Creates an instance of StorageTable.
        /// </summary>
        /// <param name="endpoint">A Uri referencing the table service account. </param>
        /// <param name="credential">The shared key credential used to sign requests.</param>
        /// <param name="logger">Optional lLogger for observability.</param>
        public StorageTable(Uri endpoint, TableSharedKeyCredential credential, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new TableServiceClient(endpoint, credential);
        }

        /// <summary>
        /// Creates an instance of StorageTable.
        /// </summary>
        /// <param name="endpoint">A Uri referencing the table service account. </param>
        /// <param name="credential">The TokenCredential used to authorize requests.</param>
        /// <param name="options">Optional client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional lLogger for observability.</param>
        public StorageTable(Uri endpoint, TokenCredential credential, TableClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new TableServiceClient(endpoint, credential, options);
        }

        /// <summary>
        /// Creates an instance of StorageTable.
        /// </summary>
        /// <param name="endpoint">A Uri referencing the table service account. </param>
        /// <param name="credential">The shared key credential used to sign requests.</param>
        /// <param name="options">Optional client options that define the transport pipeline policies for authentication, retries, etc., that are applied to every request.</param>
        /// <param name="logger">Optional lLogger for observability.</param>
        public StorageTable(Uri endpoint, TableSharedKeyCredential credential, TableClientOptions options = null, ILogger logger = null)
            : this(logger)
        {
            serviceClient = new TableServiceClient(endpoint, credential, options);
        }

        /// <summary>
        /// Creates an instance of StorageTable.
        /// </summary>
        /// <param name="logger">ILogger for the application.</param>
        protected StorageTable(ILogger logger = null)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Gets a list of tables from table storage.
        /// </summary>
        /// <param name="filter">Returns only items that satisfy the filter expression.</param>
        /// <param name="maxPerPage">The maximum number of entities that will be returned per page.</param>
        /// <param name="cancellationToken">A CancellationToken controlling the request lifetime.</param>
        /// <returns>List of TableItem</returns>
        public List<TableItem> ListTables(string filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default)
        {
            return serviceClient.Query(filter, maxPerPage, cancellationToken).ToList();
        }

        /// <summary>
        /// Get a list of tables from table storage.
        /// </summary>
        /// <param name="filter">Returns only items that satisfy the filter expression.</param>
        /// <param name="maxPerPage">The maximum number of entities that will be returned per page.</param>
        /// <param name="cancellationToken">A CancellationToken controlling the request lifetime.</param>
        /// <returns>List of TableItem</returns>
        public async Task<List<TableItem>> ListTablesAsync(string filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default)
        {
            AsyncPageable<TableItem> items = serviceClient.QueryAsync(filter, maxPerPage, cancellationToken);
            List<TableItem> list = new();
            await foreach (TableItem item in items)
            {
                list.Add(item);
            }

            return list;
        }

        /// <summary>
        /// Creates a table if it does not already exist.
        /// </summary>
        /// <param name="tableName">Name of table to create.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if table created or exists; otherwise false.</returns>
        public async Task<bool> CreateTableIsNotExistsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            Response<TableItem> item = await serviceClient.CreateTableIfNotExistsAsync(tableName, cancellationToken);
            bool result = item != null;
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
            Response response = await serviceClient.DeleteTableAsync(tableName, cancellationToken);
            bool result = response != null;
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
        public async Task<Response> AddEntityAsync(string tableName, ITableEntity entity, CancellationToken cancellationToken = default)
        {
            TableClient tableClient = serviceClient.GetTableClient(tableName);
            Response response = await tableClient.AddEntityAsync(entity, cancellationToken);
            logger?.LogTrace(new EventId(95050, "StorageTable.AddEntityAsync"), "Table entity with partition key {key} inserted into table {name}.", entity.PartitionKey, tableName);
            return response;
        }

        /// <summary>
        /// Updates an azure storage table entity.
        /// </summary>
        /// <param name="tableName">Azure storage table name</param>
        /// <param name="entity">Entity to store</param>
        /// <param name="cancellationToken">Token to cancel request.</param>
        /// <returns>Azure response object.</returns>
        public async Task<Response> UpsertEntityAsync(string tableName, ITableEntity entity, CancellationToken cancellationToken = default)
        {
            TableClient tableClient = serviceClient.GetTableClient(tableName);
            Response response = await tableClient.UpsertEntityAsync(entity, default, cancellationToken);
            logger?.LogTrace(new EventId(95060, "StorageTable.InsertOrMergeEntityAsync"), "Table entity with partition key {key} inserted or merged into table {name}", entity.PartitionKey, tableName);
            return response;
        }

        /// <summary>
        /// Creates a new table operation that deletes the given entity from a table.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="entity">Entity to delete.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>TableResult</returns>
        public async Task<Response> DeleteEntityAsync(string tableName, ITableEntity entity, CancellationToken cancellationToken = default)
        {
            TableClient tableClient = serviceClient.GetTableClient(tableName);
            Response response = await tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, default, cancellationToken);

            logger?.LogTrace(new EventId(95090, "StorageTable.DeleteEntityAsync"), "Table entity with partition key {key} deleted from table {name}.", entity.PartitionKey, tableName);
            return response;
        }

        /// <summary>
        /// Gets an entity from a table.
        /// </summary>
        /// <typeparam name="T">Generic Entity Type</typeparam>
        /// <param name="tableName">Azure Table storage name</param>
        /// <param name="partitionKey">Azure table storage partition key</param>
        /// <param name="rowKey">The key for the row to get</param>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <returns>Requested entity as an object.</returns>
        public async Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey, CancellationToken cancellationToken = default)
            where T : class, ITableEntity, new()
        {
            TableClient tableClient = serviceClient.GetTableClient(tableName);
            Response<T> response = await tableClient.GetEntityAsync<T>(partitionKey, rowKey, null, cancellationToken);
            return response?.Value;
        }

        /// <summary>
        /// Queries entities from a table based on an equal operation where either/or partition key or row key is matched.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="tableName">Name of table.</param>
        /// <param name="filter">Optional string containing a filter.</param>
        /// <param name="maxPerPage">Optional number of entities returned per page.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>TableQuerySegment</returns>
        public List<T> QueryTable<T>(string tableName, string filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default)
            where T : class, Azure.Data.Tables.ITableEntity, new()
        {
            TableClient tableClient = serviceClient.GetTableClient(tableName);
            return tableClient.Query<T>(filter, maxPerPage, null, cancellationToken).ToList<T>();
        }
    }
}
