using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using HA4IoT.Contracts.Api.Cloud;

namespace HA4IoT.CloudApi.Services
{
    public class ControllerContext
    {
        private readonly TimeSpan _requestTimeToLive = TimeSpan.FromMinutes(1);
        private readonly Dictionary<Guid, MessageContext> _pendingRequests = new Dictionary<Guid, MessageContext>();
        private readonly AutoResetEvent _pendingRequestsAutoResetEvent = new AutoResetEvent(false);

        public WaitForRequestsResult WaitForRequests(TimeSpan timeout)
        {
            return _pendingRequestsAutoResetEvent.WaitOne(timeout) ? WaitForRequestsResult.RequestsAvailable : WaitForRequestsResult.NoRequestsAvailable;
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
            var requestMessage = new CloudRequestMessage();
            requestMessage.Header.CorrelationId = Guid.NewGuid();
            requestMessage.Request.Action = request.Action;
            requestMessage.Request.Parameter = request.Parameter;

            var messageContext = new MessageContext(requestMessage);

            lock (_pendingRequests)
            {
                CleanupRequests();

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
                    Trace.WriteLine($"Found no pending request for response with correlation ID '{response.Header.CorrelationId}'.");
                    return;
                }

                pendingMessage.Close(response);
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