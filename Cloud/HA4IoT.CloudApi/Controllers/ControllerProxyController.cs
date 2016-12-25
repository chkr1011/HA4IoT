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
        private Guid _controllerId;

        public ControllerProxyController(ControllerMessageDispatcher messageDispatcher)
        {
            if (messageDispatcher == null) throw new ArgumentNullException(nameof(messageDispatcher));

            _messageDispatcher = messageDispatcher;
        }

        public async Task<ApiResponse> SendRequest([FromBody] ApiRequest request)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                ValidateControllerSecurity();

                var messageContext = _messageDispatcher.EnqueueRequest(_controllerId, request);

                if (await Task.WhenAny(messageContext.Task, Task.Delay(_timeout)) != messageContext.Task)
                {
                    throw new HttpResponseException(HttpStatusCode.GatewayTimeout);
                }

                return messageContext.ResponseMessage.Response;
            }
            catch (Exception exception)
            {
                Trace.WriteLine("EXCEPTION:" + exception);
                throw;
            }
        }

        public List<CloudRequestMessage> ReceiveRequests()
        {
            ValidateControllerSecurity();

            Trace.TraceInformation($"Controller '{_controllerId}' is requesting pending requests.");
            return _messageDispatcher.GetPendingMessages(_controllerId);
        }

        public void SendResponse([FromBody] CloudResponseMessage response)
        {
            ValidateControllerSecurity();
            _messageDispatcher.EnqueueResponse(_controllerId, response);
        }

        private void ValidateControllerSecurity()
        {
            if (!Request.Headers.Contains(CloudConnectorHeaders.ApiKey))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var sentApiKey = Request.Headers.GetValues(CloudConnectorHeaders.ApiKey).First();
            if (!string.Equals(sentApiKey, ConfigurationManager.AppSettings["ApiKey"]))
            {
                //throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            if (!Request.Headers.Contains(CloudConnectorHeaders.ControllerId))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            _controllerId = Guid.Parse(Request.Headers.GetValues(CloudConnectorHeaders.ControllerId).First());
            // TODO: Check controller ID whitelist.
        }
    }
}