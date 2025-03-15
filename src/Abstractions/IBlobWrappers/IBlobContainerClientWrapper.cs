namespace FlowStorage.Abstractions.IBlobWrappers
{
    internal interface IBlobContainerClientWrapper
    {
        Task CreateIfNotExistsAsync();
        Task DeleteIfExistsAsync();
        bool Exists();
        IBlobClientWrapper GetBlobClient(string blobName);
    }
}