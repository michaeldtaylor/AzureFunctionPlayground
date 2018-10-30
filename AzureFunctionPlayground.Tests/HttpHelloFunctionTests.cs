using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using NUnit.Framework;
using Shouldly;

namespace AzureFunctionPlayground.Tests
{
    public class HttpHelloFunctionTests
    {
        [TestCase("Michael", "\"Hello Michael\"")]
        [TestCase("Fred", "\"Hello Fred\"")]
        public async Task When_sending_a_name_then_says_hello_name(string name, string expectedResponse)
        {
            // Arrange
            var log = new TestLogger();
            var requestUri = $"http://example.org/?name={name}";
            
            // Act
            var httpResponseMessage = await HttpHelloFunction.Run(CreateHttpRequestMessage(requestUri), log);

            // Assert
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.ShouldBe(expectedResponse);
        }

        private static HttpRequestMessage CreateHttpRequestMessage(string requestUri)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            var httpConfiguration = new HttpConfiguration();

            httpRequestMessage.SetConfiguration(httpConfiguration);
            
            return httpRequestMessage;
        }
    }
}
