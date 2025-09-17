namespace FlowStorage.Abstractions.Services
{
    internal interface IAzureBlobFlowStorage : IFlowStorage
    {
        string GenerateSaSUri(string containerName, string filePath, DateTimeOffset expiryTime);
    }
}
