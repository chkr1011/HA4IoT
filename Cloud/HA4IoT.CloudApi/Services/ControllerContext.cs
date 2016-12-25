using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HA4IoT.Contracts.Api.Cloud;

namespace HA4IoT.CloudApi.Services
{
    public class ControllerContext
    {
        private readonly TimeSpan _requestTimeToLive = TimeSpan.FromMinutes(1);
        private readonly Guid _controllerId;
        private readonly Dictionary<Guid, MessageContext> _pendingRequests = new Dictionary<Guid, MessageContext>();
        private readonly AutoResetEvent _pendingRequestsAutoResetEvent = new AutoResetEvent(false);

        public ControllerContext(Guid controllerId)
        {
            _controllerId = controllerId;
        }

        public void WaitForRequests()
        {
            lock (_pendingRequests)
            {
                if (_pendingRequests.Values.Count(pr => !pr.IsDelivered) > 0)
                {
                    return;
                }
            }

            _pendingRequestsAutoResetEvent.WaitOne();
        }

        public List<CloudRequestMessage> GetPendingRequests()
        {
            lock (_pendingRequests)
            {
                var pendingRequests = _pendingRequests.Values.Where(pr => !pr.IsDelivered).ToList();
                foreach (var pendingRequest in pendingRequests)
                {
                    pendingRequest.IsDelivered = true;
                }

                return pendingRequests.Select(pr => pr.RequestMessage).ToList();
            }
        }

        public MessageContext EnqueueRequest(ApiRequest request)
        {
            lock (_pendingRequests)
            {
                CleanupRequests();

                var requestMessage = new CloudRequestMessage();
                requestMessage.Header.ControllerId = _controllerId;
                requestMessage.Header.CorrelationId = Guid.NewGuid();
                requestMessage.Request.Action = request.Action;
                requestMessage.Request.Parameter = request.Parameter;

                var messageContext = new MessageContext(requestMessage);
                _pendingRequests.Add(requestMessage.Header.CorrelationId, messageContext);

                _pendingRequestsAutoResetEvent.Set();

                return messageContext;
            }
        }

        public void EnqueueResponse(CloudResponseMessage response)
        {
            lock (_pendingRequests)
            {
                MessageContext pendingMessage;
                if (!_pendingRequests.TryGetValue(response.Header.CorrelationId, out pendingMessage))
                {
                    return;
                }

                pendingMessage.Close(response);
                _pendingRequests.Remove(response.Header.CorrelationId);
            }
        }

        private void CleanupRequests()
        {
            var obsoleteRequests = _pendingRequests.Where(i => i.Value.Duration > _requestTimeToLive).Select(i => i.Key);
            foreach (var obsoleteRequest in obsoleteRequests)
            {
                _pendingRequests.Remove(obsoleteRequest);
            }
        }
    }
}