namespace FlowStorage.Abstractions.IBlobWrappers
{
    internal interface IBlobServiceClientWrapper
    {
        IBlobContainerClientWrapper GetBlobContainerClient(string containerName);
    }
}
