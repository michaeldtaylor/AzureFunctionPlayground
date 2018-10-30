using System;
using System.Configuration;

namespace AddressFulfilment.Shared.Storage
{
    public static class AzureStorageAccount
    {
        private static readonly Lazy<string> DefaultConnectionString = new Lazy<string>(() => ConfigurationManager.AppSettings["StorageConnectionString"]);

        public static Lazy<string> ConnectionString { get; private set; } = DefaultConnectionString;

        public static void SetDefaultConnectionString(string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionString = new Lazy<string>(() => connectionString);
            }
        }

        public static void ResetConnectionString()
        {
            ConnectionString = DefaultConnectionString;
        }
    }
}