using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private Guid _controllerId;

        public ControllerProxyController(SecurityService securityService, ControllerMessageDispatcher messageDispatcher)
        {
            if (securityService == null) throw new ArgumentNullException(nameof(securityService));
            if (messageDispatcher == null) throw new ArgumentNullException(nameof(messageDispatcher));

            _securityService = securityService;
            _messageDispatcher = messageDispatcher;
        }

        public async Task<ApiResponse> Execute([FromBody] ApiRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            ValidateControllerSecurity();

            Trace.TraceInformation($"Received request for controller '{_controllerId}'.");
            return await _messageDispatcher.SendRequestAsync(_controllerId, request, _timeout);
        }

        public List<CloudRequestMessage> ReceiveRequests()
        {
            ValidateControllerSecurity();

            Trace.TraceInformation($"Controller '{_controllerId}' is requesting pending requests.");
            return _messageDispatcher.GetPendingMessages(_controllerId);
        }

        public void SendResponse([FromBody] CloudResponseMessage response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));

            ValidateControllerSecurity();

            Trace.TraceInformation($"Received response from controller '{_controllerId}'.");
            _messageDispatcher.EnqueueResponse(_controllerId, response);
        }

        private void ValidateControllerSecurity()
        {
            if (!Request.Headers.Contains(CloudConnectorHeaders.ApiKey))
            {
                throw new UnauthorizedAccessException();
            }

            var sentApiKey = Request.Headers.GetValues(CloudConnectorHeaders.ApiKey).First();
            if (!_securityService.ApiKeyIsValid(sentApiKey))
            {
                throw new UnauthorizedAccessException();
            }

            if (!Request.Headers.Contains(CloudConnectorHeaders.ControllerId))
            {
                throw new UnauthorizedAccessException();
            }

            _controllerId = Guid.Parse(Request.Headers.GetValues(CloudConnectorHeaders.ControllerId).First());
            if (!_securityService.ControllerIsAllowed(_controllerId))
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}