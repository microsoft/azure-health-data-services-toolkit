using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.AzureHealth.DataServices.Storage;
using Microsoft.AzureHealth.DataServices.Tests.Assets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using AzureCore = Azure;

namespace Microsoft.AzureHealth.DataServices.Tests.Storage
{
    [TestClass]
    public class TableStorageTests
    {
        private static readonly string LogPath = "../../storagetablelog.txt";
        private static readonly string Alphabet = "abcdefghijklmnopqrtsuvwxyz";
        private static StorageTable storage;
        private static Microsoft.Extensions.Logging.ILogger logger;
        private static ConcurrentQueue<string> queue;
        private static Random random;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            random = new Random();

            Console.WriteLine(context.TestName);
            string connectionString = root["StorageConnectionString"];

            queue = new ConcurrentQueue<string>();

            Serilog.Core.Logger slog = new LoggerConfiguration()
            .WriteTo.File(
            LogPath,
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

        [TestInitialize]
        public static void Initialize()
        {
            List<AzureCore.Data.Tables.Models.TableItem> result = storage.ListTables();

            foreach (AzureCore.Data.Tables.Models.TableItem item in result)
            {
                storage.DeleteTableIfExistsAsync(item.Name).GetAwaiter();
            }
        }

        [ClassCleanup]
        public static async Task CleanupTestSuite()
        {
            List<AzureCore.Data.Tables.Models.TableItem> result = storage.ListTables();
            foreach (AzureCore.Data.Tables.Models.TableItem item in result)
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
            AzureCore.Response response = await storage.UpsertEntityAsync(tableName, entity);
            Assert.IsTrue(response.Status == 204, $"Invalid status code {response.Status}.");
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
            List<AzureCore.Data.Tables.Models.TableItem> result = storage.ListTables();
            List<string> names = new List<string>();
            foreach (AzureCore.Data.Tables.Models.TableItem item in result)
            {
                names.Add(item.Name);
            }

            Assert.IsTrue(names.Contains($"test{tableName1}"), $"Table name test{tableName1} not found.");
            Assert.IsTrue(names.Contains($"test{tableName2}"), $"Table name test{tableName2} not found.");
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
            _ = await storage.UpsertEntityAsync(tableName, entity);

            List<MyEntity> result = storage.QueryTable<MyEntity>(tableName, $"PartitionKey eq '{partitionKey}'");
            Assert.IsTrue(result.Count == 1, "Unexpected number of entities.");
            Assert.AreEqual(result[0].PartitionKey, partitionKey, "PartitionKey mismatch.");
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
            _ = await storage.AddEntityAsync(tableName, entity);

            string filter = $"PartitionKey eq '{partitionKey}'";
            List<MyEntity> result = storage.QueryTable<MyEntity>(tableName, filter);

            Assert.IsTrue(result.Count == 1, "Unexpected number of entities.");
            Assert.AreEqual(result[0].PartitionKey, partitionKey, "PartitionKey mismatch.");
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
            AzureCore.Response result = await storage.UpsertEntityAsync(tableName, entity);
            Assert.IsTrue(result.Status == 204);
        }

        [TestMethod]
        public async Task Table_UpsertEntityAsync()
        {
            string tableName = GetRandomName();
            queue.Enqueue(tableName);
            _ = await storage.CreateTableIsNotExistsAsync(tableName);
            string partitionKey = "somePartition";
            string rowKey = "1";
            var entity = new TableEntity(partitionKey, rowKey);
            AzureCore.Response response = await storage.UpsertEntityAsync(tableName, entity);
            Assert.IsTrue(response.Status == 204, "entity not inserted or updated.");
        }

        [TestMethod]
        public async Task Table_MergeEntityAsync()
        {
            string tableName = GetRandomName();
            queue.Enqueue(tableName);
            _ = await storage.CreateTableIsNotExistsAsync(tableName);
            var entity = new MyEntity("key1", "mid1", "John", "Smith");
            _ = await storage.UpsertEntityAsync(tableName, entity);

            // _ = await storage.InsertOrMergeEntityAsync(tableName, entity);
            entity.LastName = "Smith2";
            entity.ETag = new AzureCore.ETag("*");
            AzureCore.Response result2 = await storage.UpsertEntityAsync(tableName, entity);
            Assert.IsTrue(result2.Status == 204, "entity not merged");
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
                ETag = new AzureCore.ETag("*"),
            };
            _ = await storage.UpsertEntityAsync(tableName, entity);
            AzureCore.Response result = await storage.DeleteEntityAsync(tableName, entity);
            Assert.IsTrue(result.Status == 204, "delete entity.");
        }

        [TestMethod]
        public async Task Table_ReplaceEntityAsync()
        {
            string tableName = GetRandomName();
            queue.Enqueue(tableName);
            _ = await storage.CreateTableIsNotExistsAsync(tableName);
            var entity = new MyEntity("key1", "mid1", "John", "Smith");
            _ = await storage.UpsertEntityAsync(tableName, entity);
            entity.LastName = "Smith2";
            AzureCore.Response result = await storage.UpsertEntityAsync(tableName, entity);
            Assert.IsTrue(result.Status == 204, "entity not replaced.");
        }

        private static string GetRandomName()
        {
            StringBuilder builder = new();
            int i = 0;
            while (i < 10)
            {
                builder.Append(Convert.ToString(Alphabet.ToCharArray()[random.Next(0, 25)]));
                i++;
            }

            return builder.ToString();
        }
    }
}
