using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;

namespace Microsoft.AzureHealth.DataServices.Tests.Storage
{
    [TestClass]
    public class FileStorageTests
    {
        private static readonly string Alphabet = "abcdefghijklmnopqrtsuvwxyz";
        private static readonly string ShareName = "myshare";
        private static readonly string LogPath = "../../storagefilelog.txt";
        private static Random random;
        private static ConcurrentQueue<string> containers;
        private static StorageFiles storage;
        private static Microsoft.Extensions.Logging.ILogger logger;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Console.WriteLine(context.TestName);
            random = new();
            ConfigurationBuilder builder = new();
            builder.AddUserSecrets<FileStorageTests>(true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            string connectionString = root["StorageConnectionString"];

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
            storage = new(connectionString, logger);
            storage.CreateShareIfNotExistsAsync(ShareName).GetAwaiter().GetResult();
        }

        [TestInitialize]
        public static async Task Initialize()
        {
            while (!containers.IsEmpty)
            {
                if (containers.TryDequeue(out string container))
                {
                    List<string> list = await storage.ListFilesAsync(ShareName, container);
                    foreach (var item in list)
                    {
                        await storage.DeleteFileIfExistsAsync(ShareName, container, item);
                    }

                    await storage.DeleteDirectoryIfExistsAsync(ShareName, container);
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
                    List<string> list = await storage.ListFilesAsync(ShareName, container);
                    foreach (var item in list)
                    {
                        await storage.DeleteFileIfExistsAsync(ShareName, container, item);
                    }

                    await storage.DeleteDirectoryIfExistsAsync(ShareName, container);
                }
            }
        }

        [TestMethod]
        public async Task Files_CreateDirectory_Test()
        {
            string dir = GetRandomName();
            containers.Enqueue(dir);
            await storage.CreateDirectoryIfNotExistsAsync(ShareName, dir);
            List<string> list = await storage.ListDirectoriesAsync(ShareName, null);
            Assert.IsTrue(list.Contains(dir), $"Directory {dir} not found.");
        }

        [TestMethod]
        public async Task Files_WriteFile_Test()
        {
            string dir = GetRandomName();
            string filename = $"{GetRandomName()}.txt";
            containers.Enqueue(dir);
            await storage.CreateDirectoryIfNotExistsAsync(ShareName, dir);
            string expected = "hi";
            byte[] content = Encoding.UTF8.GetBytes(expected);
            await storage.WriteFileAsync(ShareName, dir, filename, content);
            byte[] actualBytes = await storage.ReadFileAsync(ShareName, dir, filename);
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
            await storage.CreateDirectoryIfNotExistsAsync(ShareName, dir);
            string expected = "hi";
            byte[] content = Encoding.UTF8.GetBytes(expected);
            await storage.WriteFileAsync(ShareName, dir, filename1, content);
            await storage.WriteFileAsync(ShareName, dir, filename2, content);
            List<string> list = await storage.ListFilesAsync(ShareName, dir);
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
                builder.Append(Convert.ToString(Alphabet.ToCharArray()[random.Next(0, 25)]));
                i++;
            }

            return builder.ToString();
        }
    }
}
