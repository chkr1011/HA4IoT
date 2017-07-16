using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.WebApp.Controllers
{
    public class ApiController : System.Web.Http.ApiController
    {
        private const string Uri = "https://ha4iot-cloudapi.azurewebsites.net/api/ControllerProxy/Execute";

        public async Task<HttpResponseMessage> Execute(string body = null)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "");

                var response = await httpClient.PostAsync(Uri, new StringContent(body, Encoding.UTF8, "application/json"));
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = response.Content };
            }
        }
    }
}