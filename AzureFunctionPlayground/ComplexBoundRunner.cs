using System;
using System.Threading.Tasks;
using AddressFulfilment.Shared.Storage.Contracts;
using AzureFunctionPlayground.Shared.Entities;
using AzureFunctionPlayground.Shared.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureFunctionPlayground
{
    public class ComplexBoundRunner
    {
        private readonly IAzureTableStore<InputTableEntity> _inputTable;
        private readonly IAzureBlobStore _inputBlobStore;
        private readonly IAsyncCollector<OutputQueueMessage> _outputQueue;
        private readonly IAsyncCollector<OutputTableEntity> _outputTable;
        private readonly ILogger _log;

        public ComplexBoundRunner(
            IAzureTableStore<InputTableEntity> inputTable,
            IAzureBlobStore inputBlobStore,
            IAsyncCollector<OutputQueueMessage> outputQueue,
            IAsyncCollector<OutputTableEntity> outputTable,
            ILogger log)
        {
            _inputTable = inputTable;
            _inputBlobStore = inputBlobStore;
            _outputQueue = outputQueue;
            _outputTable = outputTable;
            _log = log;
        }

        public async Task RunAsync(InputQueueMessage inputQueueMessage)
        {
            var timestamp = DateTime.UtcNow;

            _log.LogInformation("The following names are valid:");

            foreach (var inputTableEntity in await _inputTable.GetAllAsync())
            {
                _log.LogInformation($"{inputTableEntity.Gender}/{inputTableEntity.Name}");
            }

            _log.LogInformation($"Reading from the input blob. blob_name={inputQueueMessage.Name}, blob_container_name={_inputBlobStore.ContainerName}");
            
            var inputBlob = await _inputBlobStore.GetBlobAsync(inputQueueMessage.Name);
            string inputBlobContent;

            try
            {
                inputBlobContent = await inputBlob.DownloadTextAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"The blob could not be found. blob_name={inputQueueMessage.Name}, blob_container_name={_inputBlobStore.ContainerName}", ex);
            }

            _log.LogInformation(inputBlobContent);

            _log.LogInformation("Writing to output queue.");

            await _outputQueue.AddAsync(new OutputQueueMessage
            {
                Message = $"Hello {inputQueueMessage.Name}",
                CreatedAt = timestamp
            });

            _log.LogInformation("Writing to output table.");

            await _outputTable.AddAsync(new OutputTableEntity
            {
                Message = $"Hello {inputQueueMessage.Name}",
                CreatedAt = timestamp
            });
        }
    }
}