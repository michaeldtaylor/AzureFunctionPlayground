using System;
using System.Threading.Tasks;
using AzureFunctionPlayground.Shared.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureFunctionPlayground
{
    public static class QueueHelloFunction
    {
        public static class QueueHelloConfiguration
        {
            // Queues
            public const string InputQueueName = "queuehelloinputqueue";
            public const string OutputQueueName = "queuehellooutputqueue";
        }

        [FunctionName(nameof(QueueHelloFunction))]
        public static async Task RunAsync(
            // Input
            [QueueTrigger(QueueHelloConfiguration.InputQueueName)] InputQueueMessage inputQueueMessage,
            [Queue(QueueHelloConfiguration.OutputQueueName)] IAsyncCollector<OutputQueueMessage> outputQueue,
            // Misc
            ILogger log)
        {
            log.LogInformation("C# queue function processed a request.");

            await outputQueue.AddAsync(new OutputQueueMessage
            {
                Message = $"Hello {inputQueueMessage.Name}",
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
