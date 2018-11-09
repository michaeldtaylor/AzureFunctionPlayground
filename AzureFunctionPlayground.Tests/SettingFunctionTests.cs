using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Shouldly;

namespace AzureFunctionPlayground.Tests
{
    [TestClass]
    public class SettingFunctionTests
    {
        [TestMethod]
        public async Task When_changing_a_setting()
        {
            using (var client = new HttpClient())
            {
                // Arrange
                var applicationSetting = new ApplicationSetting
                {
                    Name = "Mike",
                    Value = "Changed Value"
                };

                var json = JsonConvert.SerializeObject(applicationSetting);

                // Act
                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "https://azurefunctionplayground.azurewebsites.net/api/SetApplicationSetting")
                {
                    Content = new StringContent(json)
                });

                response.StatusCode.ShouldBe(HttpStatusCode.OK);

                var message = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"https://azurefunctionplayground.azurewebsites.net/api/GetApplicationSetting?name={applicationSetting.Name}"));

                var messageString = await message.Content.ReadAsStringAsync();

                messageString.ShouldBe(applicationSetting.Value);
            }
        }
    }
}
