using System.Threading.Tasks;
using AddressFulfilment.Shared.Storage.Blob;
using AddressFulfilment.Shared.Storage.Table;
using AzureFunctionPlayground.Shared.Entities;
using AzureFunctionPlayground.Shared.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureFunctionPlayground
{
    public static class ComplexBoundConfiguration
    {
        // Queues
        public const string InputQueueName = "complexboundinputqueue";
        public const string OutputQueueName = "complexboundoutputqueue";

        // Tables
        public const string InputTableName = "ComplexBoundInputTable";
        public const string OutputTableName = "ComplexBoundOutputTable";

        // Blob Containers
        public const string InputBlobContainerName = "complex-bound-blobs";
    }

    public static class ComplexBoundFunction
    {
        [FunctionName(nameof(ComplexBoundFunction))]
        public static async Task RunAsync(
            // Input
            [QueueTrigger(ComplexBoundConfiguration.InputQueueName)] InputQueueMessage inputQueueMessage,
            [Table(ComplexBoundConfiguration.InputTableName)] CloudTable inputTable,
            [Blob(ComplexBoundConfiguration.InputBlobContainerName)] CloudBlobContainer inputBlobContainer,
            // Output
            [Queue(ComplexBoundConfiguration.OutputQueueName)] IAsyncCollector<OutputQueueMessage> outputQueue,
            [Table(ComplexBoundConfiguration.OutputTableName)] IAsyncCollector<OutputTableEntity> outputTable,
            // Misc
            ILogger log)
        {
            log.LogInformation($"{nameof(ComplexBoundFunction)} processed a queue message. name={inputQueueMessage.Name}");

            var inputTableStore = new AzureTableStore<InputTableEntity>(inputTable);
            var azureBlobStore = new AzureBlobStore(inputBlobContainer);
            var complexBoundRunner = new ComplexBoundRunner(inputTableStore, azureBlobStore, outputQueue, outputTable, log);
            
            await complexBoundRunner.RunAsync(inputQueueMessage);
        }
    }
}