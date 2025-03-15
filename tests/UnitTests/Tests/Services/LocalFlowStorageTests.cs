using FlowStorage.Services;
using FlowStorage;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions.TestingHelpers;

namespace FlowStorageTests.UnitTests.Tests.Services
{
    public class LocalFlowStorageTests
    {
        private static LocalFlowStorage CreateLocalFlowStorage(string basePath, MockFileSystem? fileSystem = null, ILogger<IFlowStorage>? logger = null)
        {
            fileSystem ??= new MockFileSystem(new Dictionary<string, MockFileData>());
            logger ??= new Mock<ILogger<IFlowStorage>>().Object;
            return new LocalFlowStorage(basePath, fileSystem, logger);
        }

        [Fact]
        public async Task CreateContainerIfNotExistsAsync_CreatesDirectory_WhenNotExists()
        {
            // Arrange
            var basePath = @"C:\storage";
            var containerName = "container1";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            var localFlowStorage = CreateLocalFlowStorage(basePath, fileSystem);

            // Act
            await localFlowStorage.CreateContainerIfNotExistsAsync(containerName);

            // Assert: La carpeta del contenedor debe existir.
            Assert.True(fileSystem.Directory.Exists(Path.Combine(basePath, containerName)));
        }

        [Fact]
        public async Task CreateContainerIfNotExistsAsync_ThrowsException_WhenContainerAlreadyExists()
        {
            // Arrange
            var basePath = @"C:\storage";
            var containerName = "container1";
            // Simulamos que el contenedor ya existe creando un archivo en ese directorio.
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { Path.Combine(basePath, containerName, "dummy.txt"), new MockFileData("contenido") }
            });
            var localFlowStorage = CreateLocalFlowStorage(basePath, fileSystem);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => localFlowStorage.CreateContainerIfNotExistsAsync(containerName));
        }

        [Fact]
        public async Task CopyFileAsync_CopiesFile_WhenSourceExists()
        {
            // Arrange
            var basePath = @"C:\storage";
            var containerName = "container1";
            var sourceFile = "source.txt";
            var destFile = "dest.txt";
            var sourceContent = "¡FlowStorage is amazing!";
            var initialFiles = new Dictionary<string, MockFileData>
            {
                { Path.Combine(basePath, containerName, sourceFile), new MockFileData(sourceContent) }
            };
            var fileSystem = new MockFileSystem(initialFiles);
            // Aseguramos que el contenedor exista.
            fileSystem.AddDirectory(Path.Combine(basePath, containerName));
            var localFlowStorage = CreateLocalFlowStorage(basePath, fileSystem);

            // Act
            await localFlowStorage.CopyFileAsync(containerName, sourceFile, destFile);

            // Assert
            string destPath = Path.Combine(basePath, containerName, destFile);
            Assert.True(fileSystem.FileExists(destPath));
            var copiedContent = fileSystem.File.ReadAllText(destPath);
            Assert.Equal(sourceContent, copiedContent);
        }

        [Fact]
        public async Task CopyFileAsync_ThrowsException_WhenSourceFileDoesNotExist()
        {
            // Arrange
            var basePath = @"C:\storage";
            var containerName = "container1";
            var sourceFile = "nonexistent.txt";
            var destFile = "dest.txt";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            fileSystem.AddDirectory(Path.Combine(basePath, containerName));
            var localFlowStorage = CreateLocalFlowStorage(basePath, fileSystem);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => localFlowStorage.CopyFileAsync(containerName, sourceFile, destFile));
        }

        [Fact]
        public async Task ReadFileAsync_ReturnsContent_WhenFileExists()
        {
            // Arrange
            var basePath = @"C:\storage";
            var containerName = "container1";
            var fileName = "file.txt";
            var expectedContent = "¡FlowStorage is amazing!";
            var initialFiles = new Dictionary<string, MockFileData>
            {
                { Path.Combine(basePath, containerName, fileName), new MockFileData(expectedContent) }
            };
            var fileSystem = new MockFileSystem(initialFiles);
            fileSystem.AddDirectory(Path.Combine(basePath, containerName));
            var localFlowStorage = CreateLocalFlowStorage(basePath, fileSystem);

            // Act
            var content = await localFlowStorage.ReadFileAsync(containerName, fileName);

            // Assert
            Assert.Equal(expectedContent, content);
        }

        [Fact]
        public async Task DownloadFileAsync_ReturnsStream_WhenFileExists()
        {
            // Arrange
            var basePath = @"C:\storage";
            var containerName = "container1";
            var fileName = "file.txt";
            var fileContent = "¡FlowStorage is amazing!";
            var initialFiles = new Dictionary<string, MockFileData>
            {
                { Path.Combine(basePath, containerName, fileName), new MockFileData(fileContent) }
            };
            var fileSystem = new MockFileSystem(initialFiles);
            fileSystem.AddDirectory(Path.Combine(basePath, containerName));
            var localFlowStorage = CreateLocalFlowStorage(basePath, fileSystem);

            // Act
            using var stream = await localFlowStorage.DownloadFileAsync(containerName, fileName);
            using var reader = new StreamReader(stream);
            var downloadedContent = await reader.ReadToEndAsync();

            // Assert
            Assert.Equal(fileContent, downloadedContent);
        }

        [Fact]
        public async Task DeleteFileAsync_DeletesFile_WhenFileExists()
        {
            // Arrange
            var basePath = @"C:\storage";
            var containerName = "container1";
            var fileName = "file.txt";
            var initialFiles = new Dictionary<string, MockFileData>
            {
                { Path.Combine(basePath, containerName, fileName), new MockFileData("¡FlowStorage is amazing!") }
            };
            var fileSystem = new MockFileSystem(initialFiles);
            fileSystem.AddDirectory(Path.Combine(basePath, containerName));
            var localFlowStorage = CreateLocalFlowStorage(basePath, fileSystem);

            // Act
            await localFlowStorage.DeleteFileAsync(containerName, fileName);

            // Assert
            Assert.False(fileSystem.FileExists(Path.Combine(basePath, containerName, fileName)));
        }

        [Fact]
        public async Task DeleteContainerAsync_DeletesContainer_WhenContainerExists()
        {
            // Arrange
            var basePath = @"C:\storage";
            var containerName = "container1";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            var containerPath = Path.Combine(basePath, containerName);
            fileSystem.AddDirectory(containerPath);
            var localFlowStorage = CreateLocalFlowStorage(basePath, fileSystem);

            // Act
            await localFlowStorage.DeleteContainerAsync(containerName);

            // Assert: La carpeta del contenedor ya no debe existir.
            Assert.False(fileSystem.Directory.Exists(containerPath));
        }

        [Fact]
        public async Task UploadFileAsync_WithString_WritesFileContent()
        {
            // Arrange
            var basePath = @"C:\storage";
            var containerName = "container1";
            var fileName = "file.txt";
            var contentToUpload = "¡FlowStorage is amazing!";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            fileSystem.AddDirectory(Path.Combine(basePath, containerName));
            var localFlowStorage = CreateLocalFlowStorage(basePath, fileSystem);

            // Act
            await localFlowStorage.UploadFileAsync(containerName, fileName, contentToUpload);

            // Assert
            string filePath = Path.Combine(basePath, containerName, fileName);
            Assert.True(fileSystem.FileExists(filePath));
            var uploadedContent = fileSystem.File.ReadAllText(filePath);
            Assert.Equal(contentToUpload, uploadedContent);
        }

        [Fact]
        public async Task UploadFileAsync_WithStream_WritesFileContent()
        {
            // Arrange
            var basePath = @"C:\storage";
            var containerName = "container1";
            var fileName = "file.txt";
            var contentToUpload = "¡FlowStorage is amazing!";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            fileSystem.AddDirectory(Path.Combine(basePath, containerName));
            var localFlowStorage = CreateLocalFlowStorage(basePath, fileSystem);
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
            {
                writer.Write(contentToUpload);
                writer.Flush();
                memoryStream.Position = 0;
            }

            // Act
            await localFlowStorage.UploadFileAsync(containerName, fileName, memoryStream);

            // Assert
            string filePath = Path.Combine(basePath, containerName, fileName);
            Assert.True(fileSystem.FileExists(filePath));
            var uploadedContent = fileSystem.File.ReadAllText(filePath);
            Assert.Equal(contentToUpload, uploadedContent);
        }
    }
}
