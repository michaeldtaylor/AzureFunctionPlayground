using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AddressFulfilment.Shared.Extensions;
using AddressFulfilment.Shared.Storage.Contracts;
using AddressFulfilment.Shared.Utilities;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AddressFulfilment.Shared.Storage.Blob
{
    public class AzureBlobStore : IAzureBlobStore
    {
        private bool _initialised;
        private CloudBlobContainer _blobContainerReference;

        public AzureBlobStore(CloudBlobContainer cloudBlobContainer)
        {
            ContainerName = cloudBlobContainer.Name;

            _blobContainerReference = cloudBlobContainer;
            _initialised = true;
        }

        public AzureBlobStore(string containerName)
        {
            ContainerName = containerName;
        }

        public string ContainerName { get; }

        protected virtual BlobContainerPublicAccessType PublicAccessType => BlobContainerPublicAccessType.Off;

        public string ConnectionString { get; set; }

        public async Task DeleteAsync(string uri)
        {
            Guard.NotNullOrEmpty(nameof(uri), uri);

            Initialise();

            uri = BlobStoreUtils.GetRelativeUri(uri, _blobContainerReference);

            var blob = _blobContainerReference.GetBlockBlobReference(uri);

            if (blob != null)
            {
                await blob.DeleteIfExistsAsync();
            }
        }

        public async Task<IDictionary<string, string>> GetAsync(string uri, Stream stream)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("Uri was empty", nameof(uri));
            }

            Initialise();

            uri = BlobStoreUtils.GetRelativeUri(uri, _blobContainerReference);
            var blob = await _blobContainerReference.GetBlobReferenceFromServerAsync(uri);

            var metaData = blob.Metadata;

            await blob.DownloadToStreamAsync(stream);

            if (stream.CanSeek)
            {
                stream.Rewind();
            }

            return metaData;
        }

        public async Task DownloadAsync(string uri, Stream stream)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("Uri was empty", nameof(uri));
            }

            Initialise();

            var blob = _blobContainerReference.GetBlobReference(BlobStoreUtils.GetRelativeUri(uri, _blobContainerReference));

            await blob.DownloadToStreamAsync(stream);

            if (stream.CanSeek)
            {
                stream.Rewind();
            }
        }

        public Task<CloudBlockBlob> GetBlobAsync(string blobName)
        {
            Guard.NotNullOrEmpty(nameof(blobName), blobName);

            try
            {
                Initialise();

                blobName = BlobStoreUtils.GetRelativeUri(blobName, _blobContainerReference);

                return Task.FromResult(_blobContainerReference.GetBlockBlobReference(blobName));
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"An exception was encountered downloading blob {blobName} from container {ContainerName}",
                    exception);
            }
        }

        public Task WriteXmlAsync<T>(string blobName, T value, bool formatted = true, IDictionary<string, string> metadata = null, IDictionary<string, string> namespaceDictionary = null)
        {
            return WriteWithMetadataAsync(
                blobName,
                blob => blob.UploadTextAsync(XmlSerializerHelper.SerializeInstance(value, namespaceDictionary, formatted)),
                blob =>
                {
                    blob.Properties.ContentType = "application/xml";
                },
                metadata);
        }

        public async Task<IDictionary<string, string>> GetMetadataAsync(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("Uri was empty", nameof(uri));
            }

            IDictionary<string, string> metaData;

            try
            {
                Initialise();

                uri = BlobStoreUtils.GetRelativeUri(uri, _blobContainerReference);
                var blob = await _blobContainerReference.GetBlobReferenceFromServerAsync(uri);

                metaData = blob.Metadata;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"An exception was encountered downloading metadata for blob {uri} from container {ContainerName}", exception);
            }

            return metaData;
        }

        public string GetSharedAccessSignature(string blobName, TimeSpan expiryTime, bool writeAccess = false)
        {
            Initialise();

            blobName = BlobStoreUtils.GetRelativeUri(blobName, _blobContainerReference);
            var blob = _blobContainerReference.GetBlockBlobReference(blobName);

            var permission = writeAccess ? SharedAccessBlobPermissions.Write : SharedAccessBlobPermissions.Read;

            var sas = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                Permissions = permission,
                SharedAccessExpiryTime = DateTime.UtcNow + expiryTime
            });

            return blob.Uri + sas;
        }

        public async Task<IEnumerable<string>> ListAsync(string prefix = default(string))
        {
            Initialise();

            var blobs = await _blobContainerReference.ListBlobsAsync(prefix, true, BlobListingDetails.None, null);

            return blobs.Select(listBlobItem => listBlobItem.Uri.ToString());
        }

        public async Task<List<IListBlobItem>> ListDirectoryAsync(string directoryName)
        {
            Initialise();

            return await _blobContainerReference.ListBlobsAsync(directoryName);
        }

        public Task<Uri> WriteAsync(Stream stream, IDictionary<string, string> metadata)
        {
            return WriteAsync(stream, metadata, Guid.NewGuid().ToString());
        }

        public async Task<Uri> WriteAsync(Stream stream, IDictionary<string, string> metadata, string blobName, string friendlyName = null, string contentType = null, string contentEncoding = null)
        {
            Initialise();

            var blob = _blobContainerReference.GetBlockBlobReference(blobName);

            if (!string.IsNullOrEmpty(friendlyName))
            {
                blob.Properties.ContentDisposition = "attachment; filename=" + friendlyName;
            }

            if (!string.IsNullOrEmpty(contentType))
            {
                blob.Properties.ContentType = contentType;
            }

            if (!string.IsNullOrEmpty(contentEncoding))
            {
                blob.Properties.ContentEncoding = contentEncoding;
            }

            blob.SetMetadata(metadata);

            await blob.UploadFromStreamAsync(stream);

            return blob.Uri;
        }

        public async Task<bool> BlobExistsAsync(string blobName)
        {
            Guard.NotNullOrEmpty(nameof(blobName), blobName);

            Initialise();

            var uri = BlobStoreUtils.GetRelativeUri(blobName, _blobContainerReference);
            var blob = _blobContainerReference.GetBlockBlobReference(uri);

            return await blob.ExistsAsync();
        }

        public async Task UpdateMetadataAsync(string blobName, IDictionary<string, string> metadata)
        {
            Initialise();

            var uri = BlobStoreUtils.GetRelativeUri(blobName, _blobContainerReference);
            var blob = _blobContainerReference.GetBlockBlobReference(uri);
            await blob.FetchAttributesAsync();

            blob.SetMetadata(metadata);

            await blob.SetMetadataAsync();
        }

        protected void Initialise()
        {
            if (!_initialised)
            {
                try
                {
                    var connectionString = ConnectionString ?? AzureStorageAccount.ConnectionString.Value;

                    _blobContainerReference = AzureStorageReferenceManager.GetBlobReference(connectionString, ContainerName, PublicAccessType);

                    _initialised = true;
                }
                catch (Exception ex)
                {
                    TraceLogger.Error(ex.Message, nameof(AzureBlobStore), ex);

                    throw;
                }
            }
        }

        private async Task WriteWithMetadataAsync(
            string blobName,
            Func<CloudBlockBlob, Task> doWrite,
            Action<CloudBlockBlob> setProperties = null,
            IDictionary<string, string> metadata = null)
        {
            var blob = await GetBlobAsync(blobName);

            setProperties?.Invoke(blob);

            if (metadata != null && metadata.Any())
            {
                foreach (var pair in metadata)
                {
                    blob.Metadata[pair.Key] = pair.Value;
                }
            }

            await doWrite(blob);
        }
    }
}