using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace AzureFunctionPlayground.Tests
{
    [TestClass]
    public class HttpHelloFunctionTests
    {
        [DataTestMethod]
        [DataRow("Michael", "Hello Michael")]
        [DataRow("Fred", "Hello Fred")]
        public async Task When_sending_a_name_then_says_hello_name(string name, string expectedResponse)
        {
            // Arrange
            var log = new TestLogger();
            var requestUri = $"http://example.org/?name={name}";
            
            // Act
            var httpResponseMessage = await HttpHelloFunction.RunAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), log);

            // Assert
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.ShouldBe(expectedResponse);
        }
    }
}
