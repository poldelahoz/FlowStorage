using FlowStorage.Factories;
using FlowStorage.Services;
using FlowStorage;
using FlowStorageTests.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Logging;

namespace FlowStorageTests.IntegrationTests.Tests
{
    [Collection("Azurite collection")]
    public class AzureBlobFlowStorageConcurrencyTests(AzuriteFixture fixture)
    {
        [Fact]
        public async Task ConcurrentUploadsAndDownloads_AzureBlobFlowStorage_ShouldSucceed()
        {
            // Arrange
            string containerName = "concurrentazurecontainer";
            var blobServiceClientFactory = new BlobServiceClientFactory();
            var logger = new LoggerFactory().CreateLogger<IFlowStorage>();
            var azureBlobFlowStorage = new AzureBlobFlowStorage(fixture.ConnectionString, blobServiceClientFactory, logger);
            await azureBlobFlowStorage.CreateContainerIfNotExistsAsync(containerName);
            int numberOfFiles = 20;

            // Act: Ejecutar subidas concurrentes
            await ExecuteConcurrentTasks(numberOfFiles, async fileIndex =>
            {
                string fileName = $"azurefile_{fileIndex}.txt";
                string content = $"Azure content for file {fileIndex}";
                await azureBlobFlowStorage.UploadFileAsync(containerName, fileName, content);
            });

            // Act: Ejecutar descargas concurrentes y recoger excepciones
            var exceptions = await ExecuteConcurrentTasks(numberOfFiles, async fileIndex =>
            {
                string fileName = $"azurefile_{fileIndex}.txt";
                string downloadedContent = await azureBlobFlowStorage.ReadFileAsync(containerName, fileName);
                Assert.Equal($"Azure content for file {fileIndex}", downloadedContent);
            }, collectExceptions: true);

            // Assert
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        private async Task<List<Exception>> ExecuteConcurrentTasks(int count, Func<int, Task> taskFunc, bool collectExceptions = false)
        {
            var tasks = new List<Task>();
            var exceptions = new List<Exception>();

            for (int i = 0; i < count; i++)
            {
                int index = i; // Capturar el valor localmente
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
