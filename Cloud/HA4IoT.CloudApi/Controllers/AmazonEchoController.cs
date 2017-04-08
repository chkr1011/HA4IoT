using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using HA4IoT.CloudApi.Services;
using HA4IoT.CloudApi.Services.Exceptions;
using HA4IoT.Contracts.Api;
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
            _messageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        }

        public async Task<HttpResponseMessage> ExecuteIntent([FromBody] SkillServiceRequest request)
        {
            try
            {
                var controllerId = _securityService.GetControllerUidFromAmazonUserId(request.Session.User.UserId);
                if (controllerId == null)
                {
                    throw new UnauthorizedAccessException();
                }

                var apiRequest = new ApiRequest
                {
                    Action = "Service/IPersonalAgentService/ProcessSkillServiceRequest",
                    Parameter = JObject.FromObject(request)
                };

                var apiResponse = await _messageDispatcher.SendRequestAsync(controllerId.Value, apiRequest, TimeSpan.FromSeconds(5));
                if (apiResponse.ResultCode == ApiResultCode.Success)
                {
                    return CreateJsonResponse(apiResponse.Result.ToObject<SkillServiceResponse>());
                }

                return CreateJsonResponse("Es ist ein Fehler aufgetreten. Du musst dich bei Christian beschweren.");
            }
            catch (ControllerNotReachableException)
            {
                return CreateJsonResponse("Ich konnte deine Wohnung leider nicht erreichen. Versuche es später noch einmal.");
            }
        }

        private HttpResponseMessage CreateJsonResponse(string answer)
        {
            var response = new SkillServiceResponse();
            response.Response.OutputSpeech.Text = answer;

            return CreateJsonResponse(response);
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