using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Proxy.Storage;
using Microsoft.Health.Fhir.Proxy.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Tests.Storage
{
    [TestClass]
    public class TableStorageTests
    {
        private static StorageTable storage;
        private static string logPath = "../../storagetablelog.txt";
        private static Microsoft.Extensions.Logging.ILogger logger;
        private static ConcurrentQueue<string> queue;
        private static readonly string alphabet = "abcdefghijklmnopqrtsuvwxyz";
        private static Random random;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), false);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            random = new Random();

            Console.WriteLine(context.TestName);
            string connectionString = string.IsNullOrEmpty(root["BlobStorageConnectionString"]) ? Environment.GetEnvironmentVariable("PROXY_STORAGE_CONNECTIONSTRING") : root["BlobStorageConnectionString"];

            queue = new ConcurrentQueue<string>();

            var slog = new LoggerConfiguration()
            .WriteTo.File(
            logPath,
            shared: true,
            flushToDiskInterval: TimeSpan.FromMilliseconds(10000))
            .MinimumLevel.Debug()
            .CreateLogger();

            ILoggerFactory factory = LoggerFactory.Create(log =>
            {
                log.SetMinimumLevel(LogLevel.Trace);
                log.AddConsole();
                log.AddSerilog(slog);
            });

            logger = factory.CreateLogger("test");
            factory.Dispose();
            storage = new(connectionString, logger);
        }

        [TestInitialize()]
        public void Initialize()
        {
            var result = storage.ListTablesAsync().GetAwaiter().GetResult();

            foreach (var item in result)
            {
                storage.DeleteTableIfExistsAsync(item.Name).GetAwaiter();
            }
        }

        [ClassCleanup]
        public static async Task CleanupTestSuite()
        {
            var result = await storage.ListTablesAsync();
            foreach (var item in result)
            {
                await storage.DeleteTableIfExistsAsync(item.Name);
            }
        }

        [TestMethod]
        public async Task Table_CreateTableAsync()
        {
            string tableName = GetRandomName();
            queue.Enqueue(tableName);
            bool result = await storage.CreateTableIsNotExistsAsync(tableName);
            Assert.IsTrue(result, "Table not created.");
        }

        [TestMethod]
        public async Task Table_DeleteTableAsync()
        {
            string tableName = GetRandomName();
            queue.Enqueue(tableName);
            _ = await storage.CreateTableIsNotExistsAsync(tableName);
            bool expected = await storage.DeleteTableIfExistsAsync(tableName);
            Assert.IsTrue(expected, "table not deleted.");
        }

        [TestMethod]
        public async Task Table_InsertAsync_Test()
        {
            string tableName = GetRandomName();
            queue.Enqueue(tableName);
            _ = await storage.CreateTableIsNotExistsAsync(tableName);

            string partitionKey = "somePartition";
            string rowKey = "1";
            var entity = new TableEntity(partitionKey, rowKey);
            var result = await storage.InsertEntityAsync(tableName, entity);
            Assert.IsTrue(result.HttpStatusCode == 204, $"Invalid status code {result.HttpStatusCode}.");
        }

        [TestMethod]
        public async Task Table_ListTables_Test()
        {
            string tableName1 = GetRandomName();
            string tableName2 = GetRandomName();
            queue.Enqueue(tableName1);
            queue.Enqueue(tableName2);
            _ = await storage.CreateTableIsNotExistsAsync($"test{tableName1}");
            _ = await storage.CreateTableIsNotExistsAsync($"test{tableName2}");
            var result = await storage.ListTablesAsync("test");
            Assert.IsTrue(result.Results.Count == 2, "Unexpected number of tables.");
        }


        [TestMethod]
        public async Task Table_QueryTable_WithQueryTest()
        {
            string tableName = GetRandomName();
            queue.Enqueue(tableName);
            _ = await storage.CreateTableIsNotExistsAsync(tableName);
            string partitionKey = "partition2";
            string rowKey = "1";
            MyEntity entity = new(partitionKey, rowKey, "FirstName", "LastName");
            _ = await storage.InsertEntityAsync(tableName, entity);
            TableQuery<MyEntity> query = new TableQuery<MyEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            var result = await storage.QueryTableAsync<MyEntity>(tableName, query);
            Assert.IsTrue(result.Results.Count == 1, "Unexpected number of entities.");
            Assert.AreEqual(result.Results[0].PartitionKey, partitionKey, "PartitionKey mismatch.");
        }

        [TestMethod]
        public async Task Table_QueryTable_WithKeyTest()
        {
            string tableName = GetRandomName();
            queue.Enqueue(tableName);
            _ = await storage.CreateTableIsNotExistsAsync(tableName);
            string partitionKey = "partition1";
            string rowKey = "1";
            MyEntity entity = new(partitionKey, rowKey, "FirstName", "LastName");
            _ = await storage.InsertEntityAsync(tableName, entity);
            TableQuery<MyEntity> query = new TableQuery<MyEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            var result = await storage.QueryTableAsync<MyEntity>(tableName, partitionKey, null);
            Assert.IsTrue(result.Results.Count == 1, "Unexpected number of entities.");
            Assert.AreEqual(result.Results[0].PartitionKey, partitionKey, "PartitionKey mismatch.");
        }

        [TestMethod]
        public async Task Table_InsertOrMergeEntityAsync()
        {
            string tableName = GetRandomName();
            queue.Enqueue(tableName);
            _ = await storage.CreateTableIsNotExistsAsync(tableName);

            string partitionKey = "somePartition";
            string rowKey = "1";
            var entity = new TableEntity(partitionKey, rowKey);
            TableResult result = await storage.InsertOrMergeEntityAsync(tableName, entity);
            Assert.IsTrue(result.HttpStatusCode == 204);
        }


        [TestMethod]
        public async Task Table_InsertOrReplaceEntityAsync()
        {
            string tableName = GetRandomName();
            queue.Enqueue(tableName);
            _ = await storage.CreateTableIsNotExistsAsync(tableName);
            string partitionKey = "somePartition";
            string rowKey = "1";
            var entity = new TableEntity(partitionKey, rowKey);
            TableResult result = await storage.InsertOrReplaceEntityAsync(tableName, entity);
            Assert.IsTrue(result.HttpStatusCode == 204, "entity not inserted or replaced.");
        }

        [TestMethod]
        public async Task Table_MergeEntityAsync()
        {
            string tableName = GetRandomName();
            queue.Enqueue(tableName);
            _ = await storage.CreateTableIsNotExistsAsync(tableName);
            var entity = new MyEntity("key1", "mid1", "John", "Smith");
            _ = await storage.InsertOrMergeEntityAsync(tableName, entity);
            entity.LastName = "Smith2";
            entity.ETag = "*";
            TableResult result2 = await storage.MergeEntityAsync(tableName, entity);
            Assert.IsTrue(result2.HttpStatusCode == 204, "entity not merged");
        }

        [TestMethod]
        public async Task Table_DeleteEntityAsync()
        {
            string tableName = GetRandomName();
            queue.Enqueue(tableName);
            _ = await storage.CreateTableIsNotExistsAsync(tableName);
            string partitionKey = "somePartition";
            string rowKey = "1";
            var entity = new TableEntity(partitionKey, rowKey)
            {
                ETag = "*"
            };
            _ = await storage.InsertOrReplaceEntityAsync(tableName, entity);
            TableResult result = await storage.DeleteEntityAsync(tableName, entity);
            Assert.IsTrue(result.HttpStatusCode == 204, "delete entity.");
        }

        [TestMethod]
        public async Task Table_ReplaceEntityAsync()
        {
            string tableName = GetRandomName();
            queue.Enqueue(tableName);
            _ = await storage.CreateTableIsNotExistsAsync(tableName);
            var entity = new MyEntity("key1", "mid1", "John", "Smith");
            _ = await storage.InsertOrReplaceEntityAsync(tableName, entity);
            entity.LastName = "Smith2";
            TableResult result = await storage.ReplaceEntityAsync(tableName, entity);
            Assert.IsTrue(result.HttpStatusCode == 204, "entity not replaced.");
        }

        private static string GetRandomName()
        {
            StringBuilder builder = new();
            int i = 0;
            while (i < 10)
            {
                builder.Append(Convert.ToString(alphabet.ToCharArray()[random.Next(0, 25)]));
                i++;
            }

            return builder.ToString();
        }
    }
}
