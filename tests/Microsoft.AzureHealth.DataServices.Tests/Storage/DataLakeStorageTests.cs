using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;

namespace Microsoft.AzureHealth.DataServices.Tests.Storage
{
    // Go to Data protection and disable soft delete and use no event triggers

    [TestClass]
    public class DataLakeStorageTests
    {
        private static readonly string Alphabet = "abcdefghijklmnopqrtsuvwxyz";
        private static readonly string LogPath = "../../storagelakelog.txt";
        private static Random random;
        private static ConcurrentQueue<string> containers;
        private static StorageLake storage;
        private static string fileSystemName;
        private static ConcurrentQueue<string> filesystems;
        private static Microsoft.Extensions.Logging.ILogger logger;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            random = new();
            Console.WriteLine(context.TestName);
            fileSystemName = GetRandomName();
            ConfigurationBuilder builder = new();
            builder.AddUserSecrets<DataLakeStorageTests>(true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            string connectionString = root["StorageConnectionString"];
            containers = new();
            filesystems = new();
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
        public static async Task Initialize()
        {
            while (!containers.IsEmpty)
            {
                if (containers.TryDequeue(out string container))
                {
                    await storage.DeleteDirectoryAsync(fileSystemName, container);
                }
            }

            if (!await storage.FileSystemExistsAsync(fileSystemName))
            {
                await storage.CreateFileSystemAsync(fileSystemName);
            }
        }

        [ClassCleanup]
        public static async Task Cleanup()
        {
            while (!containers.IsEmpty)
            {
                if (containers.TryDequeue(out string container))
                {
                    await storage.DeleteDirectoryAsync(fileSystemName, container);
                }
            }

            if (await storage.FileSystemExistsAsync(fileSystemName))
            {
                await storage.DeleteFileSystemAsync(fileSystemName);
            }

            while (!filesystems.IsEmpty)
            {
                if (filesystems.TryDequeue(out string fs))
                {
                    await storage.DeleteFileSystemAsync(fs);
                }
            }
        }

        [TestMethod]
        public async Task DataLake_FileSystemExists_False_Test()
        {
            string randomName = GetRandomName();
            bool result = await storage.FileSystemExistsAsync(randomName);
            Assert.IsFalse(result, "File system should not exist.");
        }

        [TestMethod]
        public async Task DataLake_DirectoryExists_False_Test()
        {
            string path = GetRandomName();
            bool result = await storage.DirectoryExistsAsync(fileSystemName, path);
            Assert.IsFalse(result, "Directory should not exist.");
        }

        [TestMethod]
        public async Task DataLake_DirectoryExists_True_Test()
        {
            string path = GetRandomName();
            containers.Enqueue(path);
            await storage.CreateDirectoryAsync(fileSystemName, path);
            bool result = await storage.DirectoryExistsAsync(fileSystemName, path);
            Assert.IsTrue(result, "Directory should exist.");
        }

        [TestMethod]
        public async Task DataLake_FileSystemExists_True_Test()
        {
            string name = GetRandomName();
            filesystems.Enqueue(name);
            await storage.CreateFileSystemAsync(name);
            bool result = await storage.FileSystemExistsAsync(name);
            Assert.IsTrue(result, "File system not created.");
        }

        [TestMethod]
        public async Task DataLake_WriteFile_Test()
        {
            string path = GetRandomName();
            containers.Enqueue(path);
            await storage.CreateDirectoryAsync(fileSystemName, path);
            string filename = $"{GetRandomName()}.txt";
            string contentString = "hi";
            await storage.WriteFileAsync(fileSystemName, path, filename, Encoding.UTF8.GetBytes(contentString));
            byte[] actual = await storage.ReadFileAsync(fileSystemName, path, filename);
            string actualString = Encoding.UTF8.GetString(actual);
            Assert.AreEqual(contentString, actualString, "Content mismatch.");
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
