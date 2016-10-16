using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ExternalServices.AzureCloud
{
    public class AzureCloudApiDispatcherEndpointService : ServiceBase, IApiDispatcherEndpoint
    {
        private readonly IApiService _apiService;

        private EventHubSender _eventHubSender;
        private QueueSender _outboundQueue;
        private QueueReceiver _inboundQueue;

        public AzureCloudApiDispatcherEndpointService(IApiService apiService)
        {
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));

            _apiService = apiService;
        }

        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public override void Startup()
        {
            _apiService.RegisterEndpoint(this);
        }

        public void NotifyStateChanged(IComponent component)
        {
            if (_eventHubSender == null)
            {
                return;
            }

            var eventData = new JObject
            {
                ["Type"] = "StateChanged",
                ["ComponentId"] = component.Id.ToString(),
                ["State"] = component.ExportStatus()
            };

            _eventHubSender?.EnqueueEvent(eventData);
        }

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

        private void SetupEventHubSender(JsonObject settings)
        {
            // TODO: Use Options-Class
            _eventHubSender = new EventHubSender(
                settings.GetNamedString("NamespaceName"),
                settings.GetNamedString("EventHubName"),
                settings.GetNamedString("PublisherName"),
                settings.GetNamedString("Authorization"));

            _eventHubSender.Enable();
        }

        private void SetupOutboundQueueSender(JsonObject settings)
        {
            // TODO: Use Options-Class
            _outboundQueue = new QueueSender(
                settings.GetNamedString("NamespaceName"),
                settings.GetNamedString("QueueName"),
                settings.GetNamedString("Authorization"));
        }

        private void SetupInboundQueue(JsonObject settings)
        {
            // TODO: Use Options-Class
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
            var correlationId = (string)e.Body["CorrelationId"];
            var uri = (string)e.Body["Uri"];
            var request = (JObject)e.Body["Content"] ?? new JObject();

            var context = new QueueBasedApiContext(correlationId, uri, request, new JObject());
            var eventArgs = new ApiRequestReceivedEventArgs(context);
            RequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                context.ResultCode = ApiResultCode.UnknownUri;
            }

            SendResponseMessage(context).Wait();
        }

        private async Task SendResponseMessage(QueueBasedApiContext context)
        {
            var clientEtag = (string)context.Request["ETag"];

            var brokerProperties = new JObject
            {
                ["CorrelationId"] = context.CorrelationId
            };

            var message = new JObject
            {
                ["ResultCode"] = context.ResultCode.ToString(),
                ["Content"] = context.Response
        };

            var serverEtag = (string)context.Response["Meta"]["Hash"];
            message["ETag"] = serverEtag;

            if (!string.Equals(clientEtag, serverEtag))
            {
                message["Content"] = context.Response;
            }

            await _outboundQueue.SendAsync(brokerProperties, message);
        }
    }
}
