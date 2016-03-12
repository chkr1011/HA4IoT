using System;
using System.IO;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Api.AzureCloud
{
    public class AzureCloudApiDispatcherEndpoint : IApiDispatcherEndpoint
    {
        private readonly IController _controller;
        private readonly ILogger _logger;

        private EventHubSender _eventHubSender;

        public AzureCloudApiDispatcherEndpoint(IController controller, ILogger logger)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _controller = controller;
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

                _eventHubSender = new EventHubSender(
                    settings.GetNamedString("NamespaceName"),
                    settings.GetNamedString("EventHubName"),
                    settings.GetNamedString("SasToken"),
                    _logger);
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
    }
}
