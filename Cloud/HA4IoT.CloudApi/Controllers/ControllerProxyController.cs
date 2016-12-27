using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using HA4IoT.CloudApi.Services;
using HA4IoT.Contracts.Api.Cloud;

namespace HA4IoT.CloudApi.Controllers
{
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

        public async Task<ApiResponse> SendRequest([FromBody] ApiRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            ValidateControllerSecurity();
            try
            {
                Trace.TraceInformation($"Received request for controller '{_controllerId}'.");

                return await _messageDispatcher.SendRequestAsync(_controllerId, request, _timeout);
            }
            catch (TimeoutException)
            {
                throw new HttpResponseException(HttpStatusCode.BadGateway);
            }
            catch (Exception exception)
            {
                Trace.WriteLine("EXCEPTION:" + exception);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        public List<CloudRequestMessage> ReceiveRequests()
        {
            ValidateControllerSecurity();
            try
            {
                Trace.TraceInformation($"Controller '{_controllerId}' is requesting pending requests.");
                return _messageDispatcher.GetPendingMessages(_controllerId);
            }
            catch (Exception exception)
            {
                Trace.WriteLine("EXCEPTION:" + exception);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        public void SendResponse([FromBody] CloudResponseMessage response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));

            ValidateControllerSecurity();
            try
            {
                Trace.TraceInformation($"Received response from controller '{_controllerId}'.");

                _messageDispatcher.EnqueueResponse(_controllerId, response);
            }
            catch (Exception exception)
            {
                Trace.WriteLine("EXCEPTION:" + exception);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        private void ValidateControllerSecurity()
        {
            if (!Request.Headers.Contains(CloudConnectorHeaders.ApiKey))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var sentApiKey = Request.Headers.GetValues(CloudConnectorHeaders.ApiKey).First();
            if (!string.Equals(sentApiKey, _securityService.ApiKey))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            if (!Request.Headers.Contains(CloudConnectorHeaders.ControllerId))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            _controllerId = Guid.Parse(Request.Headers.GetValues(CloudConnectorHeaders.ControllerId).First());
            if (!_securityService.AllowedControllerIds.Contains(_controllerId))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
        }
    }
}