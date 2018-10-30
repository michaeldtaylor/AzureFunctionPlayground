using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AddressFulfilment.Shared.Storage.Blob
{
    public static class CloudBlobContainerExtensions
    {
        public static Task<string> AcquireLeaseAsync(
            this CloudBlobContainer blobContainer,
            TimeSpan? leaseTime,
            string proposedLeaseId,
            AccessCondition accessCondition = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginAcquireLease(leaseTime, proposedLeaseId, accessCondition, null, null, null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task<string>.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    return blobContainer.EndAcquireLease(result);
                });
        }

        public static Task<TimeSpan> BreakLeaseAsync(
            this CloudBlobContainer blobContainer,
            TimeSpan? breakPeriod = null,
            AccessCondition accessCondition = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginBreakLease(breakPeriod, accessCondition, null, null, null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task<TimeSpan>.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    return blobContainer.EndBreakLease(result);
                });
        }

        public static Task<string> ChangeLeaseAsync(
            this CloudBlobContainer blobContainer,
            string proposedLeaseId,
            AccessCondition accessCondition = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginChangeLease(proposedLeaseId, accessCondition, null, null, null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task<string>.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    return blobContainer.EndChangeLease(result);
                });
        }

        public static Task CreateAsync(
            this CloudBlobContainer blobContainer,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginCreate(null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    blobContainer.EndCreate(result);
                });
        }

        public static Task<bool> CreateIfNotExistsAsync(
            this CloudBlobContainer blobContainer,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginCreateIfNotExists(null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task<bool>.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    return blobContainer.EndCreateIfNotExists(result);
                });
        }

        public static Task DeleteAsync(
            this CloudBlobContainer blobContainer,
            AccessCondition accessCondition = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginDelete(accessCondition, null, null, null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    blobContainer.EndDelete(result);
                });
        }

        public static Task<bool> DeleteIfExistsAsync(
            this CloudBlobContainer blobContainer,
            AccessCondition accessCondition = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginDeleteIfExists(accessCondition, null, null, null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task<bool>.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    return blobContainer.EndDeleteIfExists(result);
                });
        }

        public static Task<bool> DeleteIfExistsAsync(
            this CloudBlobContainer blobContainer,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteIfExistsAsync(blobContainer, null, cancellationToken);
        }

        public static Task<bool> ExistsAsync(
            this CloudBlobContainer blobContainer,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginExists(null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task<bool>.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    return blobContainer.EndExists(result);
                });
        }

        public static Task FetchAttributesAsync(
            this CloudBlobContainer blobContainer,
            AccessCondition accessCondition = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginFetchAttributes(accessCondition, null, null, null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    blobContainer.EndFetchAttributes(result);
                });
        }

        public static Task<ICloudBlob> GetBlobReferenceFromServerAsync(
            this CloudBlobContainer blobContainer,
            string blobName,
            AccessCondition accessCondition = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginGetBlobReferenceFromServer(blobName, accessCondition, null, null, null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task<ICloudBlob>.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    return blobContainer.EndGetBlobReferenceFromServer(result);
                });
        }

        public static Task<BlobContainerPermissions> GetPermissionsAsync(
            this CloudBlobContainer blobContainer,
            AccessCondition accessCondition = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginGetPermissions(accessCondition, null, null, null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task<BlobContainerPermissions>.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    return blobContainer.EndGetPermissions(result);
                });
        }

        public static Task<BlobResultSegment> ListBlobsSegmentedAsync(
            this CloudBlobContainer blobContainer,
            string prefix,
            bool useFlatBlobListing,
            BlobListingDetails blobListingDetails,
            int? maxResults,
            BlobContinuationToken continuationToken,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginListBlobsSegmented(prefix, useFlatBlobListing, blobListingDetails, maxResults, continuationToken, null, null, null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task<BlobResultSegment>.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    return blobContainer.EndListBlobsSegmented(result);
                });
        }

        public static Task<List<IListBlobItem>> ListBlobsAsync(
            this CloudBlobContainer blobContainer,
            string prefix,
            bool useFlatBlobListing,
            BlobListingDetails blobListingDetails,
            int? maxResults,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return ListBlobsImplAsync(blobContainer, new List<IListBlobItem>(), prefix, useFlatBlobListing, blobListingDetails, maxResults, null, cancellationToken);
        }

        public static Task<List<IListBlobItem>> ListBlobsAsync(
            this CloudBlobContainer blobContainer,
            string prefix,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return ListBlobsImplAsync(blobContainer, new List<IListBlobItem>(), prefix, false, BlobListingDetails.None, null, null, cancellationToken);
        }

        public static Task<List<IListBlobItem>> ListBlobsAsync(
            this CloudBlobContainer blobContainer,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return ListBlobsImplAsync(blobContainer, new List<IListBlobItem>(), null, false, BlobListingDetails.None, null, null, cancellationToken);
        }

        public static Task ReleaseLeaseAsync(
            this CloudBlobContainer blobContainer,
            AccessCondition accessCondition,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginReleaseLease(accessCondition, null, null, null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    blobContainer.EndReleaseLease(result);
                });
        }

        public static Task RenewLeaseAsync(
            this CloudBlobContainer blobContainer,
            AccessCondition accessCondition,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginRenewLease(accessCondition, null, null, null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    blobContainer.EndRenewLease(result);
                });
        }

        public static Task SetMetadataAsync(
            this CloudBlobContainer blobContainer,
            AccessCondition accessCondition = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginSetMetadata(accessCondition, null, null, null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    blobContainer.EndSetMetadata(result);
                });
        }

        public static Task SetPermissionsAsync(
            this CloudBlobContainer blobContainer,
            BlobContainerPermissions permissions,
            AccessCondition accessCondition = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICancellableAsyncResult asyncResult = blobContainer.BeginSetPermissions(permissions, accessCondition, null, null, null, null);
            CancellationTokenRegistration registration = cancellationToken.Register(p => asyncResult.Cancel(), null);

            return Task.Factory.FromAsync(
                asyncResult,
                result =>
                {
                    registration.Dispose();
                    blobContainer.EndSetPermissions(result);
                });
        }

        private static async Task<List<IListBlobItem>> ListBlobsImplAsync(
            this CloudBlobContainer blobContainer,
            List<IListBlobItem> cloudBlobs,
            string prefix,
            bool useFlatBlobListing,
            BlobListingDetails blobListingDetails,
            int? maxResults,
            BlobContinuationToken continuationToken,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await blobContainer.ListBlobsSegmentedAsync(prefix, useFlatBlobListing, blobListingDetails, maxResults, continuationToken, cancellationToken);

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
                return await ListBlobsImplAsync(blobContainer, cloudBlobs, prefix, useFlatBlobListing, blobListingDetails, maxResults, continuationToken, cancellationToken);
            }

            return cloudBlobs;
        }
    }
}