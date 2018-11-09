using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AddressFulfilment.Shared.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctionPlayground
{
    public static class SettingFunctions
    {
        [FunctionName("GetApplicationSetting")]
        public static Task<HttpResponseMessage> GetApplicationSettingAsync(
            // Input
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET")] HttpRequestMessage requestMessage,
            // Misc
            ILogger log)
        {
            log.LogInformation("Getting Application Setting");

            var variableKeyPair = requestMessage.GetQueryNameValuePairs().SingleOrDefault(q => string.Compare(q.Key, "name", StringComparison.OrdinalIgnoreCase) == 0);

            if (variableKeyPair.Equals(default(KeyValuePair<string, string>)))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Please pass requested application setting name in the query string")
                });
            }

            var applicationSetting = Environment.GetEnvironmentVariable(variableKeyPair.Value, EnvironmentVariableTarget.Machine);

            if (applicationSetting == null)
            {
                log.LogInformation($"Did not find application setting. name={variableKeyPair.Key}");

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent)
                {
                    Content = new StringContent($"Application setting does not exit. name={variableKeyPair.Key}")
                });
            }

            log.LogInformation($"Found application setting. name={variableKeyPair.Key}, value={variableKeyPair.Value}");

            return Task.FromResult(requestMessage.CreateResponse(HttpStatusCode.OK, new
            {
                applicationSetting
            }));
        }

        [FunctionName("SetApplicationSetting")]
        public static async Task<HttpResponseMessage> SetApplicationSettingAsync(
            // Input
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST")] HttpRequestMessage requestMessage,
            // Misc
            ILogger log)
        {
            log.LogInformation("Setting Application Setting");

            var payload = await requestMessage.Content.ReadAsStringAsync();
            
            if (string.IsNullOrEmpty(payload))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Please pass applicationSetting in the request body")
                };
            }

            var applicationSetting = JsonConvert.DeserializeObject<ApplicationSetting>(payload);

            Environment.SetEnvironmentVariable(applicationSetting.Name, applicationSetting.Value, EnvironmentVariableTarget.Machine);

            return requestMessage.CreateResponse(HttpStatusCode.OK);
        }
    }

    public class ApplicationSetting
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
