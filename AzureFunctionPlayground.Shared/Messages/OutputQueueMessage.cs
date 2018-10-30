using System;

namespace AzureFunctionPlayground.Shared.Messages
{
    public class OutputQueueMessage
    {
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}