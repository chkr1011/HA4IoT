using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Api.Cloud;

namespace HA4IoT.CloudApi.Services
{
    public class ControllerContext
    {
        private readonly Dictionary<Guid, MessageContext> _pendingRequests = new Dictionary<Guid, MessageContext>();
        private readonly AutoResetEvent _pendingRequestsEvent = new AutoResetEvent(false);

        private readonly TimeSpan _requestTimeToLive = TimeSpan.FromMinutes(1);
       
        public WaitForRequestsResult WaitForRequestsAsync(TimeSpan timeout)
        {
            if (!_pendingRequestsEvent.WaitOne(timeout))
            {
                return WaitForRequestsResult.NoRequestsAvailable;
            }

            return WaitForRequestsResult.RequestsAvailable;
        }

        public List<CloudRequestMessage> GetPendingRequests()
        {
            lock (_pendingRequests)
            {
                var pendingRequestMessages = new List<CloudRequestMessage>();
                foreach (var pendingRequest in _pendingRequests)
                {
                    if (pendingRequest.Value.IsDelivered)
                    {
                        continue;
                    }

                    pendingRequest.Value.IsDelivered = true;
                    pendingRequestMessages.Add(pendingRequest.Value.RequestMessage);
                }

                return pendingRequestMessages;
            }
        }

        public MessageContext EnqueueRequest(ApiRequest request)
        {
            var requestMessage = new CloudRequestMessage();
            requestMessage.Header.CorrelationId = Guid.NewGuid();
            requestMessage.Request.Action = request.Action;
            requestMessage.Request.Parameter = request.Parameter;

            var messageContext = new MessageContext(requestMessage);

            lock (_pendingRequests)
            {
                CleanupRequests();

                _pendingRequests.Add(requestMessage.Header.CorrelationId, messageContext);
                _pendingRequestsEvent.Set();

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
                    Trace.WriteLine($"Found no pending request for response with correlation ID '{response.Header.CorrelationId}'.");
                    return;
                }

                pendingMessage.Complete(response);
                _pendingRequests.Remove(response.Header.CorrelationId);
            }
        }

        private void CleanupRequests()
        {
            var obsoleteRequests = _pendingRequests.Where(i => i.Value.Duration > _requestTimeToLive).Select(i => i.Key).ToList();
            foreach (var obsoleteRequest in obsoleteRequests)
            {
                _pendingRequests.Remove(obsoleteRequest);
            }
        }
    }
}