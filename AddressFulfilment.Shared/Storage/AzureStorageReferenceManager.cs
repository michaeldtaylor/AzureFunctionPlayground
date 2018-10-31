using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using AddressFulfilment.Shared.Extensions;
using AddressFulfilment.Shared.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;

namespace AddressFulfilment.Shared.Storage
{
    public static class AzureStorageReferenceManager
    {
        private static readonly ConcurrentDictionary<string, CloudStorageAccount> StorageAccounts = new ConcurrentDictionary<string, CloudStorageAccount>();
        private static readonly ConcurrentDictionary<string, Task<CloudTable>> TableClients = new ConcurrentDictionary<string, Task<CloudTable>>();
        private static readonly ConcurrentDictionary<string, Task<CloudBlobContainer>> BlobClients = new ConcurrentDictionary<string, Task<CloudBlobContainer>>();

        private static readonly string SessionId = Guid.NewGuid().ToString().Substring(0, 6);
        
        public static CloudStorageAccount GetStorageAccount(string storageConnectionString)
        {
            return StorageAccounts.GetOrAdd(storageConnectionString, CloudStorageAccount.Parse);
        }

        internal static Task<CloudTable> GetTableReference(string storageConnectionString, string tableName)
        {
            Guard.NotNullOrEmpty(nameof(tableName), tableName);

            tableName = GetTableName(tableName);

            var cloudStorageAccount = GetStorageAccount(storageConnectionString);

            return TableClients.GetOrAdd(cloudStorageAccount.TableEndpoint + "/" + tableName, async _ =>
            {
                var client = cloudStorageAccount.CreateCloudTableClient();

                var tableEndpoint = ServicePointManager.FindServicePoint(cloudStorageAccount.TableEndpoint);
                tableEndpoint.UseNagleAlgorithm = false;
                tableEndpoint.ConnectionLimit = 5000; // from TomP

                client.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

                var tableReference = client.GetTableReference(tableName.ToLowerInvariant());

                try
                {
                    if (await Retry.WithDelayOnException(() => tableReference.CreateIfNotExistsAsync()))
                    {
                        TraceLogger.Info($"Created table. name={tableReference.Name}", nameof(AzureStorageReferenceManager));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not create table {tableName}", ex);
                }

                return tableReference;
            });
        }

        internal static Task<CloudBlobContainer> GetBlobReferenceAsync(string storageConnectionString, string containerName, BlobContainerPublicAccessType accessType)
        {
            Guard.NotNullOrEmpty(nameof(containerName), containerName);

            containerName = GetBlobContainerName(containerName);

            var cloudStorageAccount = GetStorageAccount(storageConnectionString);

            return BlobClients.GetOrAdd(cloudStorageAccount.TableEndpoint + "/" + containerName, async _ =>
            {
                var client = cloudStorageAccount.CreateCloudBlobClient();
                client.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

                var container = client.GetContainerReference(containerName.ToLowerInvariant());

                if (await Retry.WithDelayOnException(() => container.CreateIfNotExistsAsync()))
                {
                    var containerPermissions = new BlobContainerPermissions { PublicAccess = accessType };

                    await container.SetPermissionsAsync(containerPermissions);

                    TraceLogger.Info($"Created blob container. name={container.Name}", nameof(AzureStorageReferenceManager));
                }

                return container;
            });
        }

        private static string GetTableName(string tableName)
        {
            var result = GetStorageContainerPrefix();

            return (result + tableName).ToAlphaNumericKey().TrimTo(63);
        }

        private static string GetBlobContainerName(string containerName)
        {
            var result = GetStorageContainerPrefix();

            return (result + "-" + containerName).TrimStart('-');
        }

        private static string GetStorageContainerPrefix()
        {
            var prefix = string.Empty;

            var result = prefix.Replace("{machine-name}", Environment.MachineName);

            // the session id is primarily for tests, as tables would use a prefix in that case
            // deleting a table can take approx 40 seconds, so this adds a session id to prevent conflicts
            // it is best if machine name is the first value as the tables can be automatically cleared
            // by prefix
            result = result.Replace("{session-id}", SessionId);

            return result;
        }
    }
}