using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureFunctionPlayground.Shared.Entities
{
    public class OutputTableEntity : TableEntity
    {
        public OutputTableEntity(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
        }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
