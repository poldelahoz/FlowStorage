using System.Text;

namespace FlowStorage
{
    public interface IFlowStorage
    {
        Task<string> ReadFileAsync(string containerName, string filePath);
        Task<Stream> DownloadFileAsync(string containerName, string filePath);
        Task UploadFileAsync(string containerName, string filePath, string blobContents, Encoding? encoding = null);
        Task UploadFileAsync(string containerName, string filePath, Stream fileStream, Encoding? encoding = null);
        Task CopyFileAsync(string containerName, string sourceFilePath, string destFilePath);
        Task DeleteFileIfExistsAsync(string containerName, string filePath);
        Task<bool> CreateContainerIfNotExistsAsync(string containerName);
        Task<bool> DeleteContainerIfExistsAsync(string containerName);
        string GenerateSaSUri(string containerName, string filePath, DateTimeOffset expiryTime);
    }
}
