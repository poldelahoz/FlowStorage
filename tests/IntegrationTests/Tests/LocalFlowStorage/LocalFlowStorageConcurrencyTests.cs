
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
            var uploadTasks = new List<Task>();

            for (int i = 0; i < numberOfFiles; i++)
            {
                int fileIndex = i;
                uploadTasks.Add(Task.Run(async () =>
                {
                    string fileName = $"file_{fileIndex}.txt";
                    string content = $"Content of file {fileIndex}";
                    await _localFlowStorage.UploadFileAsync(_containerName, fileName, content);
                }));
            }
            await Task.WhenAll(uploadTasks);

            var downloadTasks = new List<Task>();
            var exceptions = new List<Exception>();

            for (int i = 0; i < numberOfFiles; i++)
            {
                int fileIndex = i;
                downloadTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        string fileName = $"file_{fileIndex}.txt";
                        using var stream = await _localFlowStorage.DownloadFileAsync(_containerName, fileName);
                        using var reader = new StreamReader(stream);
                        string content = await reader.ReadToEndAsync();
                        Assert.Equal($"Content of file {fileIndex}", content);
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }));
            }
            await Task.WhenAll(downloadTasks);

            if (exceptions.Count != 0)
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
    }
}
