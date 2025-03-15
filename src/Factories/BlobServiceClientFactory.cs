using Azure.Storage.Blobs;
using FlowStorage.Abstractions;
using FlowStorage.Abstractions.IBlobWrappers;
using FlowStorage.Wrappers;

namespace FlowStorage.Factories
{
    internal class BlobServiceClientFactory : IBlobServiceClientFactory
    {
        public IBlobServiceClientWrapper Create(string connectionString)
        {
            var client = new BlobServiceClient(connectionString);
            return new BlobServiceClientWrapper(client);
        }
    }
}
