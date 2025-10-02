﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.AzureHealth.DataServices.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;

namespace Microsoft.AzureHealth.DataServices.Tests.Storage
{
    [TestClass]
    public class QueueStorageTests
    {
        private static readonly string Alphabet = "abcdefghijklmnopqrtsuvwxyz";
        private static readonly string LogPath = "../../storagetablelog.txt";
        private static Random random;
        private static StorageQueue storage;
        private static ConcurrentQueue<string> containers;
        private static string preExistingQueue;
        private static Microsoft.Extensions.Logging.ILogger logger;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Console.WriteLine(context.TestName);
            ConfigurationBuilder builder = new();
            builder.AddUserSecrets<QueueStorageTests>(true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            string accountName = root["StorageAccountName"];
            random = new();
            containers = new();
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

            // Set environment variables for app registration if available
            if (!string.IsNullOrEmpty(root["ClientId"]) && !string.IsNullOrEmpty(root["TenantId"]) && !string.IsNullOrEmpty(root["ClientSecret"]))
            {
                Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", root["ClientId"]);
                Environment.SetEnvironmentVariable("AZURE_TENANT_ID", root["TenantId"]);
                Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", root["ClientSecret"]);
            }

            // Use Managed Identity
            var credential = new DefaultAzureCredential();

            // Initialize storage class using Managed Identity
            storage = new StorageQueue(new Uri($"https://{accountName}.queue.core.windows.net"), credential, null, logger);

            preExistingQueue = GetRandomName();
            _ = storage.CreateQueueIfNotExistsAsync(preExistingQueue).GetAwaiter().GetResult();
            containers.Enqueue(preExistingQueue);
        }

        [ClassCleanup]
        public static async Task Cleanup()
        {
            while (!containers.IsEmpty)
            {
                if (containers.TryDequeue(out string container))
                {
                    await storage.DeleteQueueIfExistsAsync(container);
                }
            }
        }

        [TestMethod]
        public async Task StorageQueue_CreateQueueIfNotExists_True_Test()
        {
            string queueName = GetRandomName();
            containers.Enqueue(queueName);
            bool result = await storage.CreateQueueIfNotExistsAsync(queueName);
            Assert.IsTrue(result, "Expected new queue.");
        }

        [TestMethod]
        public async Task StorageQueue_CreateQueueIfNotExists_False_Test()
        {
            bool result = await storage.CreateQueueIfNotExistsAsync(preExistingQueue);
            Assert.IsFalse(result, "Expected existing queue.");
        }

        [TestMethod]
        public async Task StorageQueue_DeleteQueueIfExists_True_Test()
        {
            string queueName = GetRandomName();
            containers.Enqueue(queueName);
            await storage.CreateQueueIfNotExistsAsync(queueName);
            bool result = await storage.DeleteQueueIfExistsAsync(queueName);
            Assert.IsTrue(result, "Expected queue deleted.");
        }

        [TestMethod]
        public async Task StorageQueue_DeleteQueueIfExists_False_Test()
        {
            string queueName = GetRandomName();
            bool result = await storage.DeleteQueueIfExistsAsync(queueName);
            Assert.IsFalse(result, "Expected queue did not exist.");
        }

        [TestMethod]
        public async Task StorageQueue_ListQueues_Test()
        {
            string queueName1 = GetRandomName();
            string queueName2 = GetRandomName();
            containers.Enqueue(queueName1);
            containers.Enqueue(queueName2);
            _ = await storage.CreateQueueIfNotExistsAsync(queueName1);
            _ = await storage.CreateQueueIfNotExistsAsync(queueName2);
            List<string> list = await storage.ListQueuesAsync();
            Assert.IsTrue(list.Contains(queueName1));
            Assert.IsTrue(list.Contains(queueName2));
        }

        [TestMethod]
        public async Task StorageQueue_EnqueueByteArray_Test()
        {
            string queueName = GetRandomName();
            containers.Enqueue(queueName);
            _ = await storage.CreateQueueIfNotExistsAsync(queueName);
            string msgString = "hi";
            byte[] msg = Encoding.UTF8.GetBytes(msgString);
            SendReceipt result = await storage.EnqueueAsync(queueName, msg, null, null);
            Assert.IsNotNull(result.MessageId);
        }

        [TestMethod]
        public async Task StorageQueue_EnqueueString_Test()
        {
            string queueName = GetRandomName();
            containers.Enqueue(queueName);
            _ = await storage.CreateQueueIfNotExistsAsync(queueName);
            string msg = "hi";
            SendReceipt result = await storage.EnqueueAsync(queueName, msg, null, null);
            Assert.IsNotNull(result.MessageId);
        }

        [TestMethod]
        public async Task StorageQueue_Dequeue_Test()
        {
            string queueName = GetRandomName();
            containers.Enqueue(queueName);
            _ = await storage.CreateQueueIfNotExistsAsync(queueName);
            string msg = "hi";
            _ = await storage.EnqueueAsync(queueName, msg, null, null);
            QueueMessage qmsg = await storage.DequeueAsync(queueName, null);
            Assert.AreEqual(msg, qmsg.Body.ToString(), "Message mismatch.");
        }

        [TestMethod]
        public async Task StorageQueue_DequeueBatch_Test()
        {
            string queueName = GetRandomName();
            containers.Enqueue(queueName);
            _ = await storage.CreateQueueIfNotExistsAsync(queueName);
            string msg1 = "hi-1";
            string msg2 = "hi-2";
            _ = await storage.EnqueueAsync(queueName, msg1, null, null);
            _ = await storage.EnqueueAsync(queueName, msg2, null, null);
            QueueMessage[] messages = await storage.DequeueBatchAsync(queueName, 2, null);
            Assert.AreEqual(msg1, messages[0].Body.ToString(), "Message-1 mismatch.");
            Assert.AreEqual(msg2, messages[1].Body.ToString(), "Message-2 mismatch.");
        }

        [TestMethod]
        public async Task StorageQueue_PeekMessage_Test()
        {
            string queueName = GetRandomName();
            containers.Enqueue(queueName);
            _ = await storage.CreateQueueIfNotExistsAsync(queueName);
            string msg = "hi";
            _ = await storage.EnqueueAsync(queueName, msg, null, null);
            PeekedMessage qmsg = await storage.PeekMessageAsync(queueName);
            Assert.AreEqual(msg, qmsg.Body.ToString(), "Message mismatch.");
        }

        [TestMethod]
        public async Task StorageQueue_PeekMessages_Test()
        {
            string queueName = GetRandomName();
            containers.Enqueue(queueName);
            _ = await storage.CreateQueueIfNotExistsAsync(queueName);
            string msg1 = "hi-1";
            string msg2 = "hi-2";
            _ = await storage.EnqueueAsync(queueName, msg1, null, null);
            _ = await storage.EnqueueAsync(queueName, msg2, null, null);
            PeekedMessage[] messages = await storage.PeekMessagesAsync(queueName, 2);
            Assert.AreEqual(msg1, messages[0].Body.ToString(), "Message 1 mismatch.");
            Assert.AreEqual(msg2, messages[1].Body.ToString(), "Message 2 mismatch.");
        }

        [TestMethod]
        public async Task StorageQueue_ClearMessages_Test()
        {
            string queueName = GetRandomName();
            containers.Enqueue(queueName);
            _ = await storage.CreateQueueIfNotExistsAsync(queueName);
            string msg1 = "hi-1";
            string msg2 = "hi-2";
            _ = await storage.EnqueueAsync(queueName, msg1, null, null);
            _ = await storage.EnqueueAsync(queueName, msg2, null, null);
            await storage.ClearMessagesAsync(queueName);
            PeekedMessage[] messages = await storage.PeekMessagesAsync(queueName, 10);
            Assert.IsTrue(messages.Length == 0, "Expected 0 messages.");
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
