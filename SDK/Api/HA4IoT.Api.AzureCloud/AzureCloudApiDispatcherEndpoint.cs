using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api.AzureCloud
{
    public class AzureCloudApiDispatcherEndpoint : IApiDispatcherEndpoint
    {
        private EventHubSender _eventHubSender;
        private QueueSender _outboundQueue;
        private QueueReceiver _inboundQueue;

        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public bool TryInitializeFromConfigurationFile(string filename)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            if (!File.Exists(filename))
            {
                return false;
            }

            try
            {
                var settings = JsonObject.Parse(File.ReadAllText(filename));
                var eventsSettings = settings.GetNamedObject("Events");
                SetupEventHubSender(eventsSettings);

                var outboundQueueSettings = settings.GetNamedObject("OutboundQueue");
                SetupOutboundQueueSender(outboundQueueSettings);

                var inboundQueueSettings = settings.GetNamedObject("InboundQueue");
                SetupInboundQueue(inboundQueueSettings);
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Unable to initialize AzureCloudApiDispatcherEndpoint from file.");
            }

            return false;
        }

        public void NotifyStateChanged(IComponent component)
        {
            if (_eventHubSender == null)
            {
                return;
            }

            var eventData = new JObject
            {
                ["type"] = "StateChanged",
                ["componentId"] = component.Id.ToString(),
                ["state"] = component.ExportStatus()
            };

            _eventHubSender?.EnqueueEvent(eventData);
        }

        private void SetupEventHubSender(JsonObject settings)
        {
            _eventHubSender = new EventHubSender(
                settings.GetNamedString("NamespaceName"),
                settings.GetNamedString("EventHubName"),
                settings.GetNamedString("PublisherName"),
                settings.GetNamedString("Authorization"));

            _eventHubSender.Enable();
        }

        private void SetupOutboundQueueSender(JsonObject settings)
        {
            _outboundQueue = new QueueSender(
                settings.GetNamedString("NamespaceName"),
                settings.GetNamedString("QueueName"),
                settings.GetNamedString("Authorization"));
        }

        private void SetupInboundQueue(JsonObject settings)
        {
            _inboundQueue = new QueueReceiver(
                settings.GetNamedString("NamespaceName"),
                settings.GetNamedString("QueueName"),
                settings.GetNamedString("Authorization"),
                TimeSpan.FromSeconds(60));

            _inboundQueue.MessageReceived += DistpachMessage;
            _inboundQueue.Enable();
        }

        private void DistpachMessage(object sender, MessageReceivedEventArgs e)
        {
            var processingStopwatch = Stopwatch.StartNew();

            var uri = (string)e.Body["Uri"];
            if (string.IsNullOrEmpty(uri))
            {
                Log.Warning("Received Azure queue message with missing or invalid URI property.");
                return;
            }

            var callTypeSource = (string)e.Body["CallType"];

            ApiCallType callType;
            if (!Enum.TryParse(callTypeSource, true, out callType))
            {
                Log.Warning("Received Azure queue message with missing or invalid CallType property.");
                return;
            }

            var request = (JObject)e.Body["Content"];

            var context = new QueueBasedApiContext(e.BrokerProperties, e.Body, processingStopwatch, callType, uri, request, new JObject());
            var eventArgs = new ApiRequestReceivedEventArgs(context);
            RequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                Log.Warning("Received Azure queue message is not handled.");
                return;
            }

            SendResponseMessage(context).Wait();
        }

        private async Task SendResponseMessage(QueueBasedApiContext context)
        {
            context.ProcessingStopwatch.Stop();

            var correlationId = (string)context.BrokerProperties["CorrelationId"];
            var clientEtag = (string)context.Request["ETag"];

            var brokerProperties = new JObject
            {
                ["CorrelationId"] = correlationId
            };

            var message = new JObject
            {
                ["ResultCode"] = context.ResultCode.ToString(),
                ["ProcessingDuration"] = context.ProcessingStopwatch.ElapsedMilliseconds
            };

            if (context.CallType == ApiCallType.Request)
            {
                var serverEtag = (string)context.Response["Meta"]["Hash"];
                message["ETag"] = serverEtag;

                if (!string.Equals(clientEtag, serverEtag))
                {
                    message["Content"] = context.Response;
                }
            }
            else
            {
                message["Content"] = context.Response;
            }

            await _outboundQueue.SendAsync(brokerProperties, message);
        }
    }
}
