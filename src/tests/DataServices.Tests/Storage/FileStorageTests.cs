using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DataServices.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;

namespace DataServices.Tests.Storage
{
    [TestClass]
    public class FileStorageTests
    {
        private static readonly string alphabet = "abcdefghijklmnopqrtsuvwxyz";
        private static Random random;
        private static ConcurrentQueue<string> containers;
        private static StorageFiles storage;
        private static readonly string shareName = "myshare";
        private static readonly string logPath = "../../storagefilelog.txt";
        private static Microsoft.Extensions.Logging.ILogger logger;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Console.WriteLine(context.TestName);
            random = new();
            ConfigurationBuilder builder = new();
            builder.AddUserSecrets<FileStorageTests>(true);
            var root = builder.Build();
            string connectionString = string.IsNullOrEmpty(root["BlobStorageConnectionString"]) ? Environment.GetEnvironmentVariable("PROXY_STORAGE_CONNECTIONSTRING") : root["BlobStorageConnectionString"];

            containers = new();

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
            storage.CreateShareIfNotExistsAsync(shareName).GetAwaiter().GetResult();
        }


        [TestInitialize]
        public async Task Initialize()
        {
            while (!containers.IsEmpty)
            {
                if (containers.TryDequeue(out string container))
                {
                    List<string> list = await storage.ListFilesAsync(shareName, container);
                    foreach (var item in list)
                    {
                        await storage.DeleteFileIfExistsAsync(shareName, container, item);
                    }
                    await storage.DeleteDirectoryIfExistsAsync(shareName, container);
                }
            }
        }

        [ClassCleanup]
        public static async Task Cleanup()
        {
            while (!containers.IsEmpty)
            {
                if (containers.TryDequeue(out string container))
                {
                    List<string> list = await storage.ListFilesAsync(shareName, container);
                    foreach (var item in list)
                    {
                        await storage.DeleteFileIfExistsAsync(shareName, container, item);
                    }
                    await storage.DeleteDirectoryIfExistsAsync(shareName, container);
                }
            }
        }

        [TestMethod]
        public async Task Files_CreateDirectory_Test()
        {
            string dir = GetRandomName();
            containers.Enqueue(dir);
            await storage.CreateDirectoryIfNotExistsAsync(shareName, dir);
            List<string> list = await storage.ListDirectoriesAsync(shareName, null);
            Assert.IsTrue(list.Contains(dir), $"Directory {dir} not found.");
        }

        // [TestMethod]
        // public async Task Files_DeleteDirectory_Test()
        // {
        //     string dir = GetRandomName();
        //     containers.Enqueue(dir);
        //     await storage.CreateDirectoryIfNotExistsAsync(shareName, dir);
        //     List<string> list = await storage.ListDirectoriesAsync(shareName, null);
        //     Assert.AreEqual(dir, list[0], "Directory name mismatch.");
        // }

        [TestMethod]
        public async Task Files_WriteFile_Test()
        {
            string dir = GetRandomName();
            string filename = $"{GetRandomName()}.txt";
            containers.Enqueue(dir);
            await storage.CreateDirectoryIfNotExistsAsync(shareName, dir);
            string expected = "hi";
            byte[] content = Encoding.UTF8.GetBytes(expected);
            await storage.WriteFileAsync(shareName, dir, filename, content);
            byte[] actualBytes = await storage.ReadFileAsync(shareName, dir, filename);
            string actual = Encoding.UTF8.GetString(actualBytes);
            Assert.AreEqual(expected, actual, "Content mismatch.");
        }

        [TestMethod]
        public async Task Files_ListFiles_Test()
        {
            string dir = GetRandomName();
            string filename1 = $"{GetRandomName()}.txt";
            string filename2 = $"{GetRandomName()}.txt";
            containers.Enqueue(dir);
            await storage.CreateDirectoryIfNotExistsAsync(shareName, dir);
            string expected = "hi";
            byte[] content = Encoding.UTF8.GetBytes(expected);
            await storage.WriteFileAsync(shareName, dir, filename1, content);
            await storage.WriteFileAsync(shareName, dir, filename2, content);
            List<string> list = await storage.ListFilesAsync(shareName, dir);
            Assert.IsTrue(list.Contains(filename1), "File name not found.");
            Assert.IsTrue(list.Contains(filename2), "File name not found.");
            Assert.IsTrue(list.Count == 2, $"File count mismatch was {list.Count} expected 2.");
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
