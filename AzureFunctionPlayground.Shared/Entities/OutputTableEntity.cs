using System;
using AddressFulfilment.Shared.Storage.Table;

namespace AzureFunctionPlayground.Shared.Entities
{
    public class OutputTableEntity
    {
        [PartitionKey]
        public Guid PartitionKey { get; set; }

        [RowKey]
        public Guid RowKey { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
