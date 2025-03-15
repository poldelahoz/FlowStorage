using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FlowStorageTests.UnitTests.Helpers
{
    public static class BlobDownloadResultFactory
    {
        public static BlobDownloadResult CreateFake(string content)
        {
            var blobDownloadResult = (BlobDownloadResult)Activator.CreateInstance(
                typeof(BlobDownloadResult),
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                args: null,
                culture: null)!;

            var contentProperty = typeof(BlobDownloadResult).GetProperty("Content", BindingFlags.Instance | BindingFlags.Public);
            contentProperty!.SetValue(blobDownloadResult, BinaryData.FromString(content));

            return blobDownloadResult;
        }
    }
}
