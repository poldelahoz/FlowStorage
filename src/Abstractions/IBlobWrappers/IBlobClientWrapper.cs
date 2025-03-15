using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace FlowStorage.Abstractions.IBlobWrappers
{
    internal interface IBlobClientWrapper
    {
        Task DeleteIfExistsAsync();
        Task UploadAsync(BinaryData binaryData, bool overwrite);
        Task UploadAsync(Stream stream, bool overwrite);
        Task<Azure.Response<BlobDownloadInfo>> DownloadAsync();
        Task<BlobDownloadResult> DownloadContentAsync();
        Task SyncCopyFromUriAsync(Uri sourceUri);
        Uri GenerateSasUri(BlobSasPermissions permissions, DateTimeOffset expiresOn);
    }
}
