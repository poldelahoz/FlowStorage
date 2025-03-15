using FlowStorage.Factories;
using FlowStorage.Services;
using FlowStorage;
using FlowStorageTests.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowStorageTests.IntegrationTests.Tests
{
    [Collection("Azurite collection")]
    public class AzureBlobFlowStorageConcurrencyTests
    {
        private readonly AzuriteFixture _azuriteFixture;
        private readonly AzureBlobFlowStorage _azureBlobFlowStorage;
        private readonly string _containerName = "concurrentazurecontainer";

        public AzureBlobFlowStorageConcurrencyTests(AzuriteFixture fixture)
        {
            _azuriteFixture = fixture;

            var blobServiceClientFactory = new BlobServiceClientFactory();
            var logger = new LoggerFactory().CreateLogger<IFlowStorage>();

            _azureBlobFlowStorage = new AzureBlobFlowStorage(
                _azuriteFixture.ConnectionString,
                blobServiceClientFactory,
                logger);
        }

        [Fact]
        public async Task ConcurrentUploadsAndDownloads_AzureBlobFlowStorage_ShouldSucceed()
        {
            // Arrange
            await _azureBlobFlowStorage.CreateContainerIfNotExistsAsync(_containerName);

            int numberOfFiles = 20;
            var uploadTasks = new List<Task>();

            for (int i = 0; i < numberOfFiles; i++)
            {
                int fileIndex = i;
                uploadTasks.Add(Task.Run(async () =>
                {
                    string fileName = $"azurefile_{fileIndex}.txt";
                    string content = $"Azure content for file {fileIndex}";
                    await _azureBlobFlowStorage.UploadFileAsync(_containerName, fileName, content);
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
                        string fileName = $"azurefile_{fileIndex}.txt";
                        string downloadedContent = await _azureBlobFlowStorage.ReadFileAsync(_containerName, fileName);
                        Assert.Equal($"Azure content for file {fileIndex}", downloadedContent);
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

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
