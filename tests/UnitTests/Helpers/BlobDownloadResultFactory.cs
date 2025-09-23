using Azure;
using Azure.Storage.Blobs.Models;

namespace FlowStorageTests.UnitTests.Helpers
{
    public static class BlobDownloadResultFactory
    {
        public static BlobDownloadResult CreateFake(string content, string contentType = "text/plain")
        {
            BlobDownloadDetails details = BlobsModelFactory.BlobDownloadDetails(
                lastModified: DateTimeOffset.UtcNow,
                eTag: new ETag("0x8D5E48E5EC83E28"), // Un valor de ETag de ejemplo
                contentLanguage: null,
                contentDisposition: null,
                cacheControl: null,
                contentEncoding: "utf-8", // Puedes extraerlo del contentType
                copyCompletedOn: DateTimeOffset.UtcNow,
                copyStatusDescription: null,
                copyId: null,
                copyProgress: null,
                copySource: null,
                copyStatus: CopyStatus.Success,
                contentHash: null,
                isServerEncrypted: true,
                encryptionKeySha256: null,
                encryptionScope: null,
                blobSequenceNumber: 0,
                blobCommittedBlockCount: 1,
                versionId: null,
                createdOn: DateTimeOffset.UtcNow,
                metadata: null,
                contentType: contentType 
            );

            BlobDownloadResult blobDownloadResult = BlobsModelFactory.BlobDownloadResult(
                details: details,
                content: BinaryData.FromString(content)
            );

            return blobDownloadResult;
        }
    }
}
