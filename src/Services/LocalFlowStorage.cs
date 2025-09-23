using FlowStorage.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Text;

namespace FlowStorage.Services
{
    internal sealed class LocalFlowStorage(
        string basePath,
        IFileSystem fileSystem,
        ILogger<IFlowStorage> logger) : ILocalFlowStorage
    {
        private readonly string _basePath = basePath;
        private readonly IFileSystem _fileSystem = fileSystem;
        private readonly ILogger<IFlowStorage> _logger = logger;

        public Task CopyFileAsync(string containerName, string sourceFilePath, string destFilePath)
        {
            EnsureContainerExists(containerName);
            EnsureFileExists(containerName, sourceFilePath);
            EnsurePathExists(containerName, destFilePath);

            try
            {
                string sourceFile = GetFullPath(containerName, sourceFilePath);
                string destFile = GetFullPath(containerName, destFilePath);

                _fileSystem.File.Copy(sourceFile, destFile, true);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not copy files.", ex);
            }
        }

        public Task<bool> CreateContainerIfNotExistsAsync(string containerName)
        {
            ArgumentException.ThrowIfNullOrEmpty(containerName, nameof(containerName));

            string containerPath = GetFullPath(containerName, string.Empty);

            if (_fileSystem.Directory.Exists(containerPath))
            {
                return Task.FromResult(false);
            }

            _fileSystem.Directory.CreateDirectory(containerPath);

            return Task.FromResult(true);
        }

        public Task<bool> DeleteContainerIfExistsAsync(string containerName)
        {
            ArgumentException.ThrowIfNullOrEmpty(containerName, nameof(containerName));

            try
            {
                var fullPath = GetFullPath(containerName, string.Empty);

                if (_fileSystem.Directory.Exists(fullPath))
                {
                    _fileSystem.Directory.Delete(fullPath, true);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not delete container " + containerName, ex);
            }
        }

        public Task DeleteFileIfExistsAsync(string containerName, string filePath)
        {
            EnsureContainerExists(containerName);
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            try
            {
                var fullPath = GetFullPath(containerName, filePath);

                if (_fileSystem.File.Exists(fullPath))
                {
                    _fileSystem.File.Delete(fullPath);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not delete file " + filePath, ex);
            }
        }

        public Task<Stream> DownloadFileAsync(string containerName, string filePath)
        {
            EnsureContainerExists(containerName);
            EnsureFileExists(containerName, filePath);

            try
            {
                var fullPath = GetFullPath(containerName, filePath);
                return Task.FromResult<Stream>(_fileSystem.File.OpenRead(fullPath));
            }
            catch (Exception ex)
            {
                throw new Exception("Could not download file " + filePath + " from container " + containerName, ex);
            }
        }

        public string GenerateSaSUri(string containerName, string filePath, DateTimeOffset expiryTime)
        {
            throw new NotImplementedException();
        }

        public async Task<string> ReadFileAsync(string containerName, string filePath)
        {
            EnsureContainerExists(containerName);
            EnsureFileExists(containerName, filePath);

            try
            {
                var fullPath = GetFullPath(containerName, filePath);

                using var stream = _fileSystem.File.OpenRead(fullPath);
                using StreamReader reader = new(stream);

                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Could not read file " + filePath + " from container " + containerName, ex);
            }
        }

        public async Task UploadFileAsync(string containerName, string filePath, string blobContents, Encoding? encoding = null)
        {
            EnsureContainerExists(containerName);
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));
            ArgumentException.ThrowIfNullOrEmpty(blobContents, nameof(blobContents));

            try
            {
                var fullPath = GetFullPath(containerName, filePath);

                var directory = _fileSystem.Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                {
                    ArgumentException.ThrowIfNullOrEmpty(directory, nameof(directory));

                    _fileSystem.Directory.CreateDirectory(directory);
                }

                using var stream = _fileSystem.File.OpenWrite(fullPath);
                using StreamWriter outputFile = new(stream, encoding ?? Encoding.UTF8);

                await outputFile.WriteAsync(blobContents);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not upload file " + filePath + " to container " + containerName, ex);
            }
        }

        public async Task UploadFileAsync(string containerName, string filePath, Stream fileStream, Encoding? encoding = null)
        {
            EnsureContainerExists(containerName);
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            var fullPath = GetFullPath(containerName, filePath);
            var directory = _fileSystem.Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
                ArgumentException.ThrowIfNullOrEmpty(directory, nameof(directory));
                _fileSystem.Directory.CreateDirectory(directory);
            }

            var targetEncoding = encoding ?? Encoding.UTF8;

            using var reader = new StreamReader(fileStream, targetEncoding);
            string content = await reader.ReadToEndAsync();
            await _fileSystem.File.WriteAllTextAsync(fullPath, content, targetEncoding);
        }

        #region Private Methods

        private string GetFullPath(string containerName, string filePath) => Path.Combine(_basePath, containerName, filePath);

        private bool ContainerExists(string containerName) => _fileSystem.Directory.Exists(GetFullPath(containerName, string.Empty));

        private bool FileExists(string containerName, string filePath) => _fileSystem.File.Exists(GetFullPath(containerName, filePath));

        private void EnsureContainerExists(string containerName)
        {
            ArgumentException.ThrowIfNullOrEmpty(containerName, nameof(containerName));

            if (!ContainerExists(containerName))
            {
                throw new Exception("Container " + containerName + " does not exist.");
            }
        }

        private void EnsureFileExists(string containerName, string filePath)
        {
            ArgumentException.ThrowIfNullOrEmpty(containerName, nameof(containerName));
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            if (!FileExists(containerName, filePath))
            {
                throw new Exception("File " + filePath + " does not exist in Container " + containerName + ".");
            }
        }

        private void EnsurePathExists(string containerName, string filePath)
        {
            ArgumentException.ThrowIfNullOrEmpty(containerName, nameof(containerName));
            ArgumentException.ThrowIfNullOrEmpty(filePath, nameof(filePath));

            var fullPath = GetFullPath(containerName, filePath);
            var directory = _fileSystem.Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory) && !_fileSystem.Directory.Exists(directory))
            {
                _fileSystem.Directory.CreateDirectory(directory);
            }
        }

        #endregion Private Methods
    }
}
