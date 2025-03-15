using Azure.Storage.Blobs;
using FlowStorage.Abstractions.IBlobWrappers;

namespace FlowStorage.Wrappers
{
    internal class BlobServiceClientWrapper(BlobServiceClient blobServiceClient) : IBlobServiceClientWrapper
    {
        private readonly BlobServiceClient _blobServiceClient = blobServiceClient;

        public IBlobContainerClientWrapper GetBlobContainerClient(string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            return new BlobContainerClientWrapper(containerClient);
        }
    }
}
