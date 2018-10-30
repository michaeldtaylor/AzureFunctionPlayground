using System;
using System.Threading.Tasks;
using AzureFunctionPlayground.Shared.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureFunctionPlayground
{
    public static class QueueHelloFunction
    {
        [FunctionName("QueueHelloFunction")]
        public static async Task Run(
            // Input
            [QueueTrigger("QueueHelloFunctionInput")] InputQueueMessage inputQueueMessage,
            [Queue("QueueHelloFunctionOutput")] IAsyncCollector<OutputQueueMessage> outputQueue,
            // Misc
            ILogger log)
        {
            log.LogInformation("C# queue function processed a request.");

            await outputQueue.AddAsync(new OutputQueueMessage
            {
                Message = $"Hello {inputQueueMessage.Name}",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
