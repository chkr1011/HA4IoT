using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using HA4IoT.CloudApi.Services;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Api.Cloud;
using HA4IoT.Contracts.PersonalAgent.AmazonEcho;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace HA4IoT.CloudApi.Controllers
{
    public class AmazonEchoController : ApiController
    {
        private readonly ControllerMessageDispatcher _messageDispatcher;

        public AmazonEchoController(ControllerMessageDispatcher messageDispatcher)
        {
            if (messageDispatcher == null) throw new ArgumentNullException(nameof(messageDispatcher));

            _messageDispatcher = messageDispatcher;
        }

        public async Task<HttpResponseMessage> ExecuteIntent()
        {
            try
            {
                var body = await Request.Content.ReadAsStringAsync();
                var parameter = JObject.Parse(body);

                var request = parameter.ToObject<SkillServiceRequest>();
                var controllerId = ResolveControllerId(request.Session.User.UserId);

                Trace.WriteLine($"{nameof(AmazonEchoController)}:ExecuteIntent\r\n{body}");

                var apiRequest = new ApiRequest
                {
                    Action = "Service/IPersonalAgentService/ProcessSkillServiceRequest",
                    Parameter = parameter
                };

                var apiResponse =
                    await _messageDispatcher.SendRequestAsync(controllerId, apiRequest, TimeSpan.FromSeconds(5));
                if (apiResponse.ResultCode == ApiResultCode.Success)
                {
                    return CreateJsonResponse(apiResponse.Result.ToObject<SkillServiceResponse>());
                }

                return
                    CreateJsonResponse(
                        CreateResponseWithText(
                            "Ich konnte deine Wohnung erreichen, jedoch ist ein Fehler aufgetreten. Du kannst dich gerne bei Christian beschweren."));
            }
            catch (TimeoutException)
            {
                return
                    CreateJsonResponse(
                        CreateResponseWithText(
                            "Ich konnte deine Wohnung leider nicht erreichen. Versuche es später noch einmal."));
            }
            catch (Exception exception)
            {
                Trace.TraceError("EXCEPTION:" + exception);
                throw;
            }
        }

        private SkillServiceResponse CreateResponseWithText(string text)
        {
            var response = new SkillServiceResponse();
            response.Response.OutputSpeech.Text = text;

            return response;
        }

        private HttpResponseMessage CreateJsonResponse(SkillServiceResponse skillServiceResponse)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var dataString = JsonConvert.SerializeObject(skillServiceResponse, serializerSettings);

            var content = new StringContent(dataString);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            return response;
        }

        private Guid ResolveControllerId(string amazonUserId)
        {
            // TODO: Create lookup table.

            if (!amazonUserId.Equals(
                    "amzn1.ask.account.AFUW7KUWQMSERIFZUZKIYKUIY7IQ4YR76CWXY4NTASC7PH2POXEDFZ2FO53DR7IS5VSB5HEQS747KL74RCWQQJ7BKILGXWNA6PPSMNT34COUNM7NXVJ33GSI5IMMJIOWN4NP4SBNE7EO2HYPQQLCB55FFZCNVRUFD6ZXPK2LRBSEFVYCNOML6EE4EN7D4AQNLII6UCS353CXKPA"))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            return Guid.Parse("0f39add9-bc56-4d6d-b69b-9b8b1c1ac890");
        }
    }
}