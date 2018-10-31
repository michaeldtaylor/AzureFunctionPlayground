using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AddressFulfilment.Shared.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunctionPlayground
{
    public static class HttpHelloFunction
    {
        [FunctionName(nameof(HttpHelloFunction))]
        public static async Task<HttpResponseMessage> RunAsync(
            // Input
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage requestMessage,
            // Misc
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // parse query parameter
            var name = requestMessage.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;

            if (name == null)
            {
                // Get request body
                dynamic data = await requestMessage.Content.ReadAsAsync<object>();
                name = data?.name;
            }

            return name == null
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Please pass a name on the query string or in the request body")
                }
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Hello " + name)
                };
        }
    }
}
