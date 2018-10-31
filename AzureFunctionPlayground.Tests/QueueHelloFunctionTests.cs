using System.Linq;
using System.Threading.Tasks;
using AzureFunctionPlayground.Shared.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace AzureFunctionPlayground.Tests
{
    [TestClass]
    public class QueueHelloFunctionTests
    {
        [DataTestMethod]
        [DataRow("Michael", "Hello Michael")]
        [DataRow("Fred", "Hello Fred")]
        public async Task When_adding_a_name_to_input_queue_then_adds_message_to_output_queue(string name, string expectedResponse)
        {
            // Arrange
            var log = new TestLogger();
            var outputQueue = new TestAsyncCollector<OutputQueueMessage>();
            var inputQueueMessage = new InputQueueMessage
            {
                Name = name
            };

            // Act
            await QueueHelloFunction.RunAsync(inputQueueMessage, outputQueue, log);

            // Assert
            outputQueue.AddedItems.Single().Message.ShouldBe(expectedResponse);
        }
    }
}
