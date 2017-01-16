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
    [ExceptionFilter]
    public class AmazonEchoController : ApiController
    {
        private readonly ControllerMessageDispatcher _messageDispatcher;
        private readonly SecurityService _securityService;

        public AmazonEchoController(ControllerMessageDispatcher messageDispatcher, SecurityService securityService)
        {
            if (messageDispatcher == null) throw new ArgumentNullException(nameof(messageDispatcher));
            if (securityService == null) throw new ArgumentNullException(nameof(securityService));

            _messageDispatcher = messageDispatcher;
            _securityService = securityService;
        }

        public async Task<HttpResponseMessage> ExecuteIntent()
        {
            try
            {
                var body = await Request.Content.ReadAsStringAsync();
                var parameter = JObject.Parse(body);

                var request = parameter.ToObject<SkillServiceRequest>();
                var controllerId = _securityService.GetControllerUidFromAmazonUserId(request.Session.User.UserId);
                if (controllerId == null)
                {
                    throw new UnauthorizedAccessException();
                }

                Trace.WriteLine($"{nameof(AmazonEchoController)}:ExecuteIntent\r\n{body}");

                var apiRequest = new ApiRequest
                {
                    Action = "Service/IPersonalAgentService/ProcessSkillServiceRequest",
                    Parameter = parameter
                };

                var apiResponse = await _messageDispatcher.SendRequestAsync(controllerId.Value, apiRequest, TimeSpan.FromSeconds(5));
                if (apiResponse.ResultCode == ApiResultCode.Success)
                {
                    return CreateJsonResponse(apiResponse.Result.ToObject<SkillServiceResponse>());
                }

                return
                    CreateJsonResponse(
                        CreateResponseWithText(
                            "Ich konnte deine Wohnung erreichen, jedoch ist ein Fehler aufgetreten. Du kannst dich gerne bei Christian beschweren."));
            }
            catch (ControllerNotReachableException)
            {
                return
                    CreateJsonResponse(
                        CreateResponseWithText(
                            "Ich konnte deine Wohnung leider nicht erreichen. Versuche es später noch einmal."));
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
    }
}