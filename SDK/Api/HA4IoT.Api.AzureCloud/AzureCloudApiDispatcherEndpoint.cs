using System;
using System.Diagnostics;
using System.IO;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Api.AzureCloud
{
    public class AzureCloudApiDispatcherEndpoint : IApiDispatcherEndpoint
    {
        private readonly ILogger _logger;

        private EventHubSender _eventHubSender;
        private QueueSender _outboundQueue;
        private QueueReceiver _inboundQueue;

        public AzureCloudApiDispatcherEndpoint(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

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
                _logger.Warning(exception, "Unable to initialize AzureCloudApiDispatcherEndpoint from file.");
            }

            return false;
        }

        public void NotifyStateChanged(IActuator actuator)
        {
            JsonObject eventData = new JsonObject();
            eventData.SetNamedValue("Type", "StateChanged".ToJsonValue());
            eventData.SetNamedValue("ActuatorId", actuator.Id.ToJsonValue());
            eventData.SetNamedValue("State", actuator.ExportStatusToJsonObject());

            _eventHubSender?.Send(eventData);
        }

        private void SetupEventHubSender(JsonObject settings)
        {
            _eventHubSender = new EventHubSender(
                settings.GetNamedString("NamespaceName"),
                settings.GetNamedString("EventHubName"),
                settings.GetNamedString("PublisherName"),
                settings.GetNamedString("SasToken"),
                _logger);
        }

        private void SetupOutboundQueueSender(JsonObject settings)
        {
            _outboundQueue = new QueueSender(
                settings.GetNamedString("NamespaceName"),
                settings.GetNamedString("QueueName"),
                settings.GetNamedString("SasToken"),
                _logger);
        }

        private void SetupInboundQueue(JsonObject settings)
        {
            _inboundQueue = new QueueReceiver(
                settings.GetNamedString("NamespaceName"),
                settings.GetNamedString("QueueName"),
                settings.GetNamedString("SasToken"),
                TimeSpan.FromSeconds(60),
                _logger);

            _inboundQueue.MessageReceived += DistpachMessage;
            _inboundQueue.Start();
        }

        private void DistpachMessage(object sender, MessageReceivedEventArgs e)
        {
            Stopwatch processingStopwatch = Stopwatch.StartNew();

            string uri = e.Body.GetNamedString("Uri", string.Empty);
            if (string.IsNullOrEmpty(uri))
            {
                _logger.Warning("Received Azure queue message with missing or invalid URI property.");
                return;
            }

            string callTypeSource = e.Body.GetNamedString("CallType", string.Empty);

            ApiCallType callType;
            if (!Enum.TryParse(callTypeSource, true, out callType))
            {
                _logger.Warning("Received Azure queue message with missing or invalid CallType property.");
                return;
            }

            var request = e.Body.GetNamedObject("Content", new JsonObject());

            var context = new QueueBasedApiContext(e.BrokerProperties, e.Body, processingStopwatch, callType, uri, request, new JsonObject());
            var eventArgs = new ApiRequestReceivedEventArgs(context);
            RequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                _logger.Warning("Received Azure queue message is not handled.");
                return;
            }

            SendResponseMessage(context);
        }

        private void SendResponseMessage(QueueBasedApiContext context)
        {
            context.ProcessingStopwatch.Stop();

            string correlationId = context.BrokerProperties.GetNamedString("CorrelationId", string.Empty);
            string clientEtag = context.Request.GetNamedString("ETag", string.Empty);
            
            var brokerProperties = new JsonObject();
            brokerProperties.SetNamedValue("CorrelationId", JsonValue.CreateStringValue(correlationId));

            var message = new JsonObject();
            message.SetNamedValue("ResultCode", JsonValue.CreateStringValue(context.ResultCode.ToString()));
            message.SetNamedValue("ProcessingDuration", JsonValue.CreateNumberValue(context.ProcessingStopwatch.ElapsedMilliseconds));

            if (context.CallType == ApiCallType.Request)
            {
                string serverEtag = context.Response.GetNamedObject("Meta", new JsonObject()).GetNamedString("Hash", string.Empty);
                message.SetNamedValue("ETag", JsonValue.CreateStringValue(serverEtag));

                if (!string.Equals(clientEtag, serverEtag))
                {
                    message.SetNamedValue("Content", context.Response);
                }
            }
            else
            {
                message.SetNamedValue("Content", context.Response);
            }

            _outboundQueue.Send(brokerProperties, message);
        }
    }
}
