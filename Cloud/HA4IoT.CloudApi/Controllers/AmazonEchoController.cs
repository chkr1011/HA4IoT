using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace HA4IoT.CloudApi.Controllers
{
    //[Authorize]
    public class AmazonEchoController : ApiController
    {
        public HttpResponseMessage Ping()
        {
            Trace.WriteLine(nameof(AmazonEchoController) + ":Ping");
            return CreateJsonResponse(new JObject {["Result"] = "Pong"});
        }

        public async Task<HttpResponseMessage> ExecuteIntent()
        {
            var body = await Request.Content.ReadAsStringAsync();
            Trace.WriteLine($"{nameof(AmazonEchoController)}:ExecuteIntent\r\n{body}");

            var request = JObject.Parse(body);
            
            // Schema: https://developer.amazon.com/public/solutions/alexa/alexa-skills-kit/docs/alexa-skills-kit-interface-reference
            var result = new
            {
                version = "1.0",
                response = new
                {
                    shouldEndSession = true,
                    outputSpeech = new
                    {
                        type = "PlainText",
                        text = "OK?"
                    }
                }
            };

            return CreateJsonResponse(JObject.FromObject(result));
        }

        private HttpResponseMessage CreateJsonResponse(JObject data)
        {
            var content = new StringContent(data.ToString());
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };
        
            return response;
        }
    }
}