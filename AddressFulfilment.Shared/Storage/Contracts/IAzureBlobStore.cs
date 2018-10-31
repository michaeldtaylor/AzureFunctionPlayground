using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AddressFulfilment.Shared.Storage.Contracts
{
    public interface IAzureBlobStore
    {
        string ContainerName { get; }

        Task DeleteAsync(string uri);

        Task<IDictionary<string, string>> GetAsync(string uri, Stream stream);

        Task<IDictionary<string, string>> GetMetadataAsync(string uri);

        Task UpdateMetadataAsync(string blobName, IDictionary<string, string> metadata);

        Task<IEnumerable<string>> ListAsync(string prefix = default(string));

        Task<List<IListBlobItem>> ListDirectoryAsync(string directoryName);

        Task<Uri> WriteAsync(Stream stream, IDictionary<string, string> metadata);

        Task<Uri> WriteAsync(Stream stream, IDictionary<string, string> metadata, string blobName, string friendlyName = null, string contentType = null, string contentEncoding = null);

        Task<string> GetSharedAccessSignatureAsync(string blobName, TimeSpan expiryTime, bool writeAccess = false);

        Task<bool> BlobExistsAsync(string blobName);

        Task<CloudBlockBlob> GetBlobAsync(string blobName);

        Task WriteXmlAsync<T>(string blobName, T value, bool formatted = true, IDictionary<string, string> metadata = null, IDictionary<string, string> namespaceDictionary = null);
    }
}