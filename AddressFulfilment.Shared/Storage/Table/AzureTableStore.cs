using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AddressFulfilment.Shared.Storage.Contracts;
using AddressFulfilment.Shared.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;

namespace AddressFulfilment.Shared.Storage.Table
{
    public class AzureTableStore<T> : IAzureTableStore<T>
    {
        private bool _initialised;
        private int _maximumBatchSize;
        private CloudTable _tableReference;

        private static readonly TableRequestOptions DefaultRequestOptions = new TableRequestOptions
        {
            RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3)
        };

        public AzureTableStore(
            string tableName,
            Func<T, string> partitionKeySelector,
            Func<T, string> rowKeySelector)
        {
            Guard.NotNull(nameof(tableName), tableName);
            Guard.NotNull(nameof(partitionKeySelector), partitionKeySelector);
            Guard.NotNull(nameof(rowKeySelector), rowKeySelector);

            TableName = tableName;

            // The selector will always normalise the keys, which ensures if 3rd parties use this they too get
            // the 'safe' key and a consistent view throughout
            PartitionKeySelector = e => GenerateKey(partitionKeySelector(e));
            RowKeySelector = e => GenerateKey(rowKeySelector(e));

            RequestOptions = DefaultRequestOptions;

            MaximumBatchSize = 100;
        }

        public AzureTableStore(CloudTable cloudTable) : this(cloudTable.Name)
        {
            _tableReference = cloudTable;
            _initialised = true;
        }

        public AzureTableStore(string tableName)
        {
            if (typeof(ITableEntity).IsAssignableFrom(typeof(T)))
            {
                PartitionKeySelector = e =>
                {
                    var tableEntity = e as ITableEntity;

                    return GenerateKey(tableEntity?.PartitionKey.ToString());
                };

                RowKeySelector = e =>
                {
                    var tableEntity = e as ITableEntity;

                    return GenerateKey(tableEntity?.RowKey.ToString());
                };
            }
            else
            {
                var properties = typeof(T).GetProperties();

                foreach (var property in properties)
                {
                    var attributes = property.GetCustomAttributes().ToList();

                    if (attributes.Any(a => a is PartitionKeyAttribute))
                    {
                        PartitionKeySelector = e => GenerateKey(property.GetValue(e).ToString());
                    }

                    if (attributes.Any(a => a is RowKeyAttribute))
                    {
                        RowKeySelector = e => GenerateKey(property.GetValue(e).ToString());
                    }
                }
            }

            if (PartitionKeySelector == null)
            {
                throw new MissingKeyAttributeException(nameof(PartitionKeyAttribute), typeof(T).Name);
            }

            if (RowKeySelector == null)
            {
                throw new MissingKeyAttributeException(nameof(RowKeyAttribute), typeof(T).Name);
            }

            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));

            RequestOptions = DefaultRequestOptions;

            MaximumBatchSize = 100;
        }

        public string TableName { get; }

        public Func<T, string> PartitionKeySelector { get; }

        public Func<T, string> RowKeySelector { get; }

        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TableRequestOptions" /> that are to be used in each call made,
        /// for example the retry policy to be used.
        /// </summary>
        public TableRequestOptions RequestOptions { get; set; }

        public int MaximumBatchSize
        {
            get => _maximumBatchSize;

            set
            {
                if (value > 100)
                {
                    throw new ArgumentException(@"Cannot set a maximum batch size greater than 100.",
                        nameof(MaximumBatchSize));
                }

                _maximumBatchSize = value;
            }
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return QueryAsync(new TableQuery());
        }

        public Task InsertAsync(T entity, bool ignoreETag = false)
        {
            return SaveAsync(entity, TableOperation.Insert, ignoreETag);
        }

        public Task ReplaceAsync(T entity, bool ignoreETag = false)
        {
            return SaveAsync(entity, TableOperation.Replace, ignoreETag);
        }

        public Task InsertOrReplaceAsync(T entity, bool ignoreETag = false)
        {
            return SaveAsync(entity, TableOperation.InsertOrReplace, ignoreETag);
        }

        public async Task DeleteAsync(T entity, bool ignoreETag = false)
        {
            await InitialiseAsync();

            var dynamicTableEntity = new DynamicTableEntity
            {
                PartitionKey = PartitionKeySelector(entity),
                RowKey = RowKeySelector(entity),
                Properties = new Dictionary<string, EntityProperty>()
            };

            if (ignoreETag)
            {
                dynamicTableEntity.ETag = "*";
            }
            else if (entity is ITableEntity)
            {
                dynamicTableEntity.ETag = (entity as ITableEntity).ETag;
            }

            await _tableReference.ExecuteAsync(
                TableOperation.Delete(dynamicTableEntity),
                RequestOptions,
                null);
        }

        public async Task<bool> TryDeleteAsync(T entity, bool ignoreETag = false)
        {
            try
            {
                await DeleteAsync(entity, ignoreETag);

                return true;
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404)
            {
                return true;
            }
            catch (Exception ex)
            {
                TraceLogger.Error("Could not delete entity.", nameof(AzureTableStore<T>), ex);

                return false;
            }
        }

        public async Task<T> GetAsync(string partitionKey, string rowKey)
        {
            await InitialiseAsync();

            var retrieveOperation = TableOperation.Retrieve(
                GenerateKey(partitionKey),
                GenerateKey(rowKey),
                EntityConverter<T>.TableEntityResolver);

            var retrievedResult = await _tableReference.ExecuteAsync(
                retrieveOperation,
                RequestOptions,
                null);

            return (T) retrievedResult.Result;
        }

        public async Task<IEnumerable<T>> QueryAsync(TableQuery query)
        {
            await InitialiseAsync();

            TableContinuationToken token = null;
            var results = new List<T>();

            do
            {
                var segment = await _tableReference.ExecuteQuerySegmentedAsync(
                    query,
                    EntityConverter<T>.TableEntityResolver,
                    token,
                    RequestOptions,
                    null);

                token = segment.ContinuationToken;

                results.AddRange(segment.Results.Cast<T>());
            } while (token != null);

            return results;
        }

        public async Task<bool> ContainsQueryAsync(TableQuery tableQuery)
        {
            await InitialiseAsync();

            // We only need to grab a single entity to know whether or not anything exists
            tableQuery.Take(1);
            tableQuery.SelectColumns = new List<string>
            {
                "PartitionKey"
            };

            var segment = await _tableReference.ExecuteQuerySegmentedAsync(
                tableQuery,
                null,
                RequestOptions,
                null);

            return segment.Results.Any();
        }

        public async Task ClearAsync()
        {
            await InitialiseAsync();

            var tableQuery = new TableQuery
            {
                SelectColumns = new List<string>
                {
                    "PartitionKey",
                    "RowKey"
                }
            };

            TableContinuationToken token = null;

            do
            {
                var segment = await _tableReference.ExecuteQuerySegmentedAsync(
                    tableQuery,
                    token,
                    RequestOptions,
                    null);

                token = segment.ContinuationToken;

                foreach (var e in segment.Results)
                {
                    e.ETag = "*";

                    await _tableReference.ExecuteAsync(
                        TableOperation.Delete(e),
                        RequestOptions,
                        null);
                }
            } while (token != null);
        }

        public string GenerateKey(string key)
        {
            return TableKeyEncoding.Encode(key);
        }

        private async Task InitialiseAsync()
        {
            if (!_initialised)
            {
                var storageConnectionString = AzureStorageAccount.ConnectionString.Value;

                if (string.IsNullOrEmpty(ConnectionString) && string.IsNullOrEmpty(storageConnectionString))
                {
                    throw new AzureTableStoreException($"Initialisation failed for AzureTableStore of type {typeof(T)}. Both ConnectionString property, and App.config StorageAccountConnectionString are null.");
                }

                _tableReference = await AzureStorageReferenceManager.GetTableReference(ConnectionString ?? storageConnectionString, TableName);
                _initialised = true;
            }
        }

        private async Task SaveAsync(
            T entity,
            Func<DynamicTableEntity, TableOperation> getTableOperation,
            bool ignoreETag = false)
        {
            await InitialiseAsync();

            var tableOperation = ToTableOperation(entity, getTableOperation, ignoreETag);

            var tableResult = await _tableReference.ExecuteAsync(
                tableOperation,
                RequestOptions,
                null);

            if (entity is ITableEntity)
            {
                (entity as ITableEntity).ETag = tableResult.Etag;
            }
        }

        private TableOperation ToTableOperation(T entity, Func<DynamicTableEntity, TableOperation> getTableOperation,
            bool ignoreETag)
        {
            var convertedEntity = EntityConverter.ConvertToDynamicTableEntity(
                entity,
                PartitionKeySelector(entity),
                RowKeySelector(entity));

            if (ignoreETag)
            {
                convertedEntity.ETag = "*";
            }

            return getTableOperation(convertedEntity);
        }
    }
}