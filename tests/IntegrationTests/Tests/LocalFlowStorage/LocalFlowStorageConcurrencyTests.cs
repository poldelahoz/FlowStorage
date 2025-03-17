
using FlowStorage.Services;
using FlowStorage;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace FlowStorageTests.IntegrationTests.Tests
{
    public class LocalFlowStorageConcurrencyTests : IDisposable
    {
        private readonly string _basePath;
        private readonly LocalFlowStorage _localFlowStorage;
        private readonly string _containerName = "concurrentcontainer";

        public LocalFlowStorageConcurrencyTests()
        {
            _basePath = Path.Combine(Path.GetTempPath(), "LocalFlowStorage_ConcurrencyTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_basePath);

            var fileSystem = new FileSystem();
            var logger = new LoggerFactory().CreateLogger<IFlowStorage>();
            _localFlowStorage = new LocalFlowStorage(_basePath, fileSystem, logger);
        }

        [Fact]
        public async Task ConcurrentUploadsAndDownloads_LocalFlowStorage_ShouldSucceed()
        {
            // Arrange
            await _localFlowStorage.CreateContainerIfNotExistsAsync(_containerName);
            int numberOfFiles = 50;

            // Act: Ejecutar subidas concurrentes
            await ExecuteConcurrentTasks(numberOfFiles, async fileIndex =>
            {
                string fileName = $"file_{fileIndex}.txt";
                string content = $"Content of file {fileIndex}";
                await _localFlowStorage.UploadFileAsync(_containerName, fileName, content);
            });

            // Act: Ejecutar descargas concurrentes y recoger excepciones
            var exceptions = await ExecuteConcurrentTasks(numberOfFiles, async fileIndex =>
            {
                string fileName = $"file_{fileIndex}.txt";
                using var stream = await _localFlowStorage.DownloadFileAsync(_containerName, fileName);
                using var reader = new StreamReader(stream);
                string content = await reader.ReadToEndAsync();
                Assert.Equal($"Content of file {fileIndex}", content);
            }, collectExceptions: true);

            // Assert: Si se recogieron excepciones, se lanzan en AggregateException
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        public void Dispose()
        {
            if (Directory.Exists(_basePath))
            {
                Directory.Delete(_basePath, recursive: true);
            }
        }

        private async Task<List<Exception>> ExecuteConcurrentTasks(int count, Func<int, Task> taskFunc, bool collectExceptions = false)
        {
            var tasks = new List<Task>();
            var exceptions = new List<Exception>();

            for (int i = 0; i < count; i++)
            {
                int index = i; // Capturamos el valor localmente
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await taskFunc(index);
                    }
                    catch (Exception ex)
                    {
                        if (collectExceptions)
                        {
                            lock (exceptions)
                            {
                                exceptions.Add(ex);
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                }));
            }
            await Task.WhenAll(tasks);
            return exceptions;
        }
    }
}
