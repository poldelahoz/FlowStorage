using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace FlowStorage.Abstractions.IBlobWrappers
{
    internal interface IBlobClientWrapper
    {
        Task<bool> ExistsAsync();
        Task DeleteIfExistsAsync();
        Task UploadAsync(BinaryData binaryData, bool overwrite = true, Encoding? encoding = null);
        Task UploadAsync(Stream stream, bool overwrite = true, Encoding? encoding = null);
        Task<Azure.Response<BlobDownloadInfo>> DownloadAsync();
        Task<BlobDownloadResult> DownloadContentAsync();
        Task<Stream> OpenReadAsync();
        Task SyncCopyFromUriAsync(Uri sourceUri);
        Uri GenerateSasUri(BlobSasPermissions permissions, DateTimeOffset expiresOn);
    }
}
