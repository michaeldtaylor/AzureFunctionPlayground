using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AddressFulfilment.Shared.Storage.Blob
{
    public static class CloudBlobContainerExtensions
    {
        public static Task<List<IListBlobItem>> ListBlobsAsync(
            this CloudBlobContainer blobContainer,
            string prefix,
            bool useFlatBlobListing = false,
            BlobListingDetails blobListingDetails = BlobListingDetails.None,
            int? maxResults = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return ListBlobsImplAsync(blobContainer, new List<IListBlobItem>(), prefix, useFlatBlobListing, blobListingDetails, maxResults, cancellationToken);
        }

        private static async Task<List<IListBlobItem>> ListBlobsImplAsync(
            this CloudBlobContainer blobContainer,
            List<IListBlobItem> cloudBlobs,
            string prefix,
            bool useFlatBlobListing,
            BlobListingDetails blobListingDetails,
            int? maxResults,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await blobContainer.ListBlobsSegmentedAsync(prefix, useFlatBlobListing, blobListingDetails, maxResults, null, null, null, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            cloudBlobs.AddRange(result.Results);

            // Checks whether maxresults entities has been received
            if (maxResults.HasValue && cloudBlobs.Count >= maxResults.Value)
            {
                return cloudBlobs.Take(maxResults.Value).ToList();
            }

            // Checks whether enumeration has been completed
            if (result.ContinuationToken != null)
            {
                return await ListBlobsImplAsync(blobContainer, cloudBlobs, prefix, useFlatBlobListing, blobListingDetails, maxResults, cancellationToken);
            }

            return cloudBlobs;
        }
    }
}