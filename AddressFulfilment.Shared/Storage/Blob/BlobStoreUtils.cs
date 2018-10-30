using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AddressFulfilment.Shared.Storage.Blob
{
    public static class BlobStoreUtils
    {
        public static string GetRelativeUri(string blobUrl, CloudBlobContainer container)
        {
            if (!blobUrl.StartsWith("http"))
            {
                return blobUrl; // already relative so just return it
            }

            // seems absolute URLs don't work anymore so grab just the LocalPath and
            // remove the name of the container
            var blobPath = new Uri(blobUrl).LocalPath;
            var containerPath = container.Uri.LocalPath;

            return blobPath.Replace(containerPath + "/", string.Empty);
        }
    }
}