using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using HA4IoT.Contracts.Api.Cloud;

namespace HA4IoT.CloudApi.Services
{
    public class ControllerMessageDispatcher
    {
        private readonly Dictionary<Guid, ControllerContext> _pendingMessages = new Dictionary<Guid, ControllerContext>();

        public List<CloudRequestMessage> GetPendingMessages(Guid controllerUid)
        {
            ControllerContext controllerContext;
            lock (_pendingMessages)
            {
                controllerContext = GetOrCreateControllerContext(controllerUid);
            }

            if (controllerContext.WaitForRequests(TimeSpan.FromMinutes(1)) == WaitForRequestsResult.NoRequestsAvailable)
            {
                return null;
            }

            return controllerContext.GetPendingRequests();
        }

        public async Task<ApiResponse> SendRequestAsync(Guid controllerId, ApiRequest request, TimeSpan timeout)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var messageContext = EnqueueRequest(controllerId, request);

            if (await Task.WhenAny(messageContext.Task, Task.Delay(timeout)) != messageContext.Task)
            {
                throw new TimeoutException();
            }

            return messageContext.ResponseMessage.Response;
        }

        public void EnqueueResponse(Guid controllerId, CloudResponseMessage response)
        {
            ControllerContext controllerContext;
            lock (_pendingMessages)
            {
                controllerContext = GetOrCreateControllerContext(controllerId);
            }

            controllerContext.EnqueueResponse(response);
        }

        private MessageContext EnqueueRequest(Guid controllerId, ApiRequest request)
        {
            ControllerContext controllerContext;
            lock (_pendingMessages)
            {
                controllerContext = GetOrCreateControllerContext(controllerId);
            }

            return controllerContext.EnqueueRequest(request);
        }

        private ControllerContext GetOrCreateControllerContext(Guid controllerId)
        {
            ControllerContext result;
            if (!_pendingMessages.TryGetValue(controllerId, out result))
            {
                Trace.WriteLine($"Created new context for controller '{controllerId}'.");
                result = new ControllerContext(controllerId);
                _pendingMessages.Add(controllerId, result);
            }

            return result;
        }
    }
}