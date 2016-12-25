using System;
using System.Collections.Generic;
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

            controllerContext.WaitForRequests();
            return controllerContext.GetPendingRequests();
        }

        public MessageContext EnqueueRequest(Guid controllerId, ApiRequest request)
        {
            ControllerContext controllerContext;
            lock (_pendingMessages)
            {
                controllerContext = GetOrCreateControllerContext(controllerId);
            }

            return controllerContext.EnqueueRequest(request);
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

        private ControllerContext GetOrCreateControllerContext(Guid controllerId)
        {
            ControllerContext result;
            if (!_pendingMessages.TryGetValue(controllerId, out result))
            {
                result = new ControllerContext(controllerId);
                _pendingMessages.Add(controllerId, result);
            }

            return result;
        }
    }
}