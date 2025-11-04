using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using FlowStorage.Abstractions.IBlobWrappers;
using Azure;
using System.Text;

namespace FlowStorage.Wrappers
{
    public class BlobClientWrapper(BlobClient blobClient) : IBlobClientWrapper
    {
        private readonly BlobClient _blobClient = blobClient;

        public async Task<bool> ExistsAsync()
        {
            return await _blobClient.ExistsAsync();
        }
        
        public async Task DeleteIfExistsAsync()
        {
            await _blobClient.DeleteIfExistsAsync();
        }

        public async Task UploadAsync(BinaryData binaryData, bool overwrite = true, Encoding? encoding = null)
        {
            var options = new BlobUploadOptions();

            if (!overwrite)
            {
                options.Conditions = new BlobRequestConditions { IfNoneMatch = new ETag("*") };
            }

            if (encoding != null)
            {
                options.HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = $"text/plain; charset={encoding.WebName}"
                };
            }

            await _blobClient.UploadAsync(binaryData, options);
        }

        public async Task UploadAsync(Stream stream, bool overwrite = true, Encoding? encoding = null)
        {
            var options = new BlobUploadOptions();

            if (!overwrite)
            {
                options.Conditions = new BlobRequestConditions { IfNoneMatch = new ETag("*") };
            }

            if (encoding != null)
            {
                options.HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = $"text/plain; charset={encoding.WebName}"
                };
            }

            await _blobClient.UploadAsync(stream, options);
        }

        public async Task<Azure.Response<BlobDownloadInfo>> DownloadAsync()
        {
            return await _blobClient.DownloadAsync();
        }

        public async Task<Stream> OpenReadAsync()
        {
            return await _blobClient.OpenReadAsync();
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
