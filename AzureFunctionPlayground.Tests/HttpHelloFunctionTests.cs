using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureFunctionPlayground.Tests
{
    [TestClass]
    public class HttpHelloFunctionTests
    {
        private TestTraceWriter _traceWriter;

        [TestInitialize]
        public void Initialize()
        {
            _traceWriter = new TestTraceWriter(TraceLevel.Error);
        }

        [TestMethod]
        [DataRow("Michael", "\"Hello Michael\"")]
        [DataRow("Fred", "\"Hello Fred\"")]
        public async Task When_sending_a_name_then_says_hello_name(string name, string expectedResponseContent)
        {
            // Arrange
            var requestUri = $"http://example.org/?name={name}";

            // Act
            var httpResponseMessage = await HttpHelloFunction.Run(CreateHttpRequestMessage(string.Empty, requestUri), _traceWriter);

            // Assert
            var content = await httpResponseMessage.Content.ReadAsStringAsync();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(expectedResponseContent, content);
        }

        private static HttpRequestMessage CreateHttpRequestMessage(string content, string requestUri)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            var httpConfiguration = new HttpConfiguration();

            httpRequestMessage.SetConfiguration(httpConfiguration);
            
            return httpRequestMessage;
        }
    }
}
