using Azure.Storage.Blobs;
using FlowStorage.Abstractions.IBlobWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowStorage.Wrappers
{
    internal class BlobContainerClientWrapper(BlobContainerClient containerClient) : IBlobContainerClientWrapper
    {
        private readonly BlobContainerClient _containerClient = containerClient;

        public bool Exists()
        {
            // Nota: Para simplificar, usamos Exists() sincrónicamente.
            // En producción quizás quieras usar la versión asíncrona.
            return _containerClient.Exists();
        }

        public IBlobClientWrapper GetBlobClient(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            return new BlobClientWrapper(blobClient);
        }

        public async Task CreateIfNotExistsAsync()
        {
            await _containerClient.CreateIfNotExistsAsync();
        }

        public async Task DeleteIfExistsAsync()
        {
            await _containerClient.DeleteIfExistsAsync();
        }
    }
}
