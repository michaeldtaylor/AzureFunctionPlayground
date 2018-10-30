using System;

namespace AddressFulfilment.Shared.Storage.Table
{
    public class AzureTableStoreException : Exception
    {
        public AzureTableStoreException(string message) : base(message)
        {
        }
    }
}