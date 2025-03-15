namespace FlowStorage
{
    public interface IFlowStorage
    {
        Task<string> ReadFileAsync(string containerName, string filePath);
        Task<Stream> DownloadFileAsync(string containerName, string filePath);
        Task UploadFileAsync(string containerName, string filePath, string blobContents);
        Task UploadFileAsync(string containerName, string filePath, Stream fileStream);
        Task CopyFileAsync(string containerName, string sourceFilePath, string destFilePath);
        Task DeleteFileAsync(string containerName, string filePath);
        Task CreateContainerIfNotExistsAsync(string containerName);
        Task DeleteContainerAsync(string containerName);
    }
}
