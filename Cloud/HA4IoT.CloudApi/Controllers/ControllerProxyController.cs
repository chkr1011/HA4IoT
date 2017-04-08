using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using HA4IoT.CloudApi.Services;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Api.Cloud;

namespace HA4IoT.CloudApi.Controllers
{
    [ExceptionFilter]
    public class ControllerProxyController : ApiController
    {
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);
        private readonly ControllerMessageDispatcher _messageDispatcher;
        private readonly SecurityService _securityService;

        public ControllerProxyController(SecurityService securityService, ControllerMessageDispatcher messageDispatcher)
        {
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
            _messageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
        }

        public Task<ApiResponse> Execute([FromBody] ApiRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            Guid controllerId;
            ValidateControllerSecurity(out controllerId);

            Trace.TraceInformation($"Received request for controller '{controllerId}'.");
            return _messageDispatcher.SendRequestAsync(controllerId, request, _timeout);
        }

        public List<CloudRequestMessage> ReceiveRequests()
        {
            Guid controllerId;
            ValidateControllerSecurity(out controllerId);

            Trace.TraceInformation($"Controller '{controllerId}' is requesting pending requests.");
            return _messageDispatcher.GetPendingMessagesAsync(controllerId);
        }

        public void SendResponse([FromBody] CloudResponseMessage response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));

            Guid controllerId;
            ValidateControllerSecurity(out controllerId);

            Trace.TraceInformation($"Received response from controller '{controllerId}'.");
            _messageDispatcher.EnqueueResponse(controllerId, response);
        }

        private void ValidateControllerSecurity(out Guid controllerId)
        {
            if (Request.Headers.Authorization == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (Request.Headers.Authorization.Scheme != "Basic")
            {
                throw new UnauthorizedAccessException();
            }

            var authorizationValue = Encoding.UTF8.GetString(Convert.FromBase64String(Request.Headers.Authorization.Parameter));
            if (!authorizationValue.Contains(":"))
            {
                throw new UnauthorizedAccessException();
            }

            var controllerIdText = authorizationValue.Substring(0, authorizationValue.IndexOf(':'));
            var apiKey = authorizationValue.Substring(controllerIdText.Length + 1);

            if (string.IsNullOrEmpty(controllerIdText) || string.IsNullOrEmpty(apiKey))
            {
                throw new UnauthorizedAccessException();
            }

            if (!Guid.TryParse(controllerIdText, out controllerId))
            {
                throw new UnauthorizedAccessException();
            }

            if (!_securityService.CredentialsAreValid(controllerId, apiKey))
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}