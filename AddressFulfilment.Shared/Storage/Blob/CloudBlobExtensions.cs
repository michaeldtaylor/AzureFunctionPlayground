using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AddressFulfilment.Shared.Storage.Blob
{
    public static class CloudBlobExtensions
    {
        public static void SetMetadata(this ICloudBlob blob, IDictionary<string, string> metaData)
        {
            if (metaData != null && metaData.Keys.Any())
            {
                if (metaData.TryGetValue("ContentType", out var contentType))
                {
                    blob.Properties.ContentType = contentType;
                }

                foreach (var pair in metaData)
                {
                    blob.Metadata[pair.Key] = pair.Value;
                }
            }
        }
    }
}