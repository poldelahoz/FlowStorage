using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using FlowStorage.Abstractions.IBlobWrappers;

namespace FlowStorage.Wrappers
{
    public class BlobClientWrapper(BlobClient blobClient) : IBlobClientWrapper
    {
        private readonly BlobClient _blobClient = blobClient;

        public async Task DeleteIfExistsAsync()
        {
            await _blobClient.DeleteIfExistsAsync();
        }

        public async Task UploadAsync(BinaryData binaryData, bool overwrite)
        {
            await _blobClient.UploadAsync(binaryData, overwrite);
        }

        public async Task UploadAsync(Stream stream, bool overwrite)
        {
            await _blobClient.UploadAsync(stream, overwrite);
        }

        public async Task<Azure.Response<BlobDownloadInfo>> DownloadAsync()
        {
            return await _blobClient.DownloadAsync();
        }

        public async Task<BlobDownloadResult> DownloadContentAsync()
        {
            return await _blobClient.DownloadContentAsync();
        }

        public async Task SyncCopyFromUriAsync(Uri sourceUri)
        {
            await _blobClient.SyncCopyFromUriAsync(sourceUri);
        }

        public Uri GenerateSasUri(BlobSasPermissions permissions, DateTimeOffset expiresOn)
        {
            return _blobClient.GenerateSasUri(permissions, expiresOn);
        }
    }
}
