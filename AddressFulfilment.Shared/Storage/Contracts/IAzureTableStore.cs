using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AddressFulfilment.Shared.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table;

namespace AddressFulfilment.Shared.Storage.Contracts
{
    /// <summary>
    /// Represents a single Azure Table, one that has no restrictions on the type of entity
    /// being stored to allow 'POCO' objects to be used.
    /// </summary>
    /// <remarks>
    /// The implementation for storing and querying for entities relies on the capabilities
    /// of <see cref="EntityConverter"/> to enable non <see cref="ITableEntity" /> classes to
    /// be stored, removing the requirement of all-public properties and also enabling
    /// the storage of complex objects as serialised JSON.
    /// </remarks>
    /// <typeparam name="T">The type of entity to store / load</typeparam>
    public interface IAzureTableStore<T>
    {
        string TableName { get; }

        Func<T, string> PartitionKeySelector { get; }

        Func<T, string> RowKeySelector { get; }

        /// <summary>
        /// Gets or sets the <see cref="TableRequestOptions" /> that are to be used in each call made,
        /// for example the retry policy to be used.
        /// </summary>
        TableRequestOptions RequestOptions { get; set; }

        int MaximumBatchSize { get; set; }

        Task<IEnumerable<T>> GetAllAsync();

        Task InsertAsync(T entity, bool ignoreETag = false);

        Task ReplaceAsync(T entity, bool ignoreETag = false);

        Task InsertOrReplaceAsync(T entity, bool ignoreETag = false);

        Task DeleteAsync(T entity, bool ignoreETag = false);

        Task<bool> TryDeleteAsync(T entity, bool ignoreETag = false);

        Task<T> GetAsync(string partitionKey, string rowKey);

        Task<IEnumerable<T>> QueryAsync(TableQuery query);

        Task<bool> ContainsQueryAsync(TableQuery tableQuery);

        Task ClearAsync();

        string GenerateKey(string key);
    }
}
