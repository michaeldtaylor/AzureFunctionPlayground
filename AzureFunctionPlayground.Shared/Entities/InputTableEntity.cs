using AddressFulfilment.Shared.Storage.Table;

namespace AzureFunctionPlayground.Shared.Entities
{
    public class InputTableEntity
    {
        [PartitionKey]
        public string Gender { get; set; }

        [RowKey]
        public string Name { get; set; }
    }
}