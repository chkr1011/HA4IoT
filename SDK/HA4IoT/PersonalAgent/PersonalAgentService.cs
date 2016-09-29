using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.PersonalAgent;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.Weather;

namespace HA4IoT.PersonalAgent
{
    [ApiServiceClass(typeof(PersonalAgentService))]
    public class PersonalAgentService : ServiceBase, IPersonalAgentService
    {
        private readonly SynonymService _synonymService;
        private readonly IComponentService _componentService;
        private readonly IAreaService _areaService;
        private readonly IWeatherService _weatherService;
        private readonly IOutdoorTemperatureService _outdoorTemperatureService;
        private readonly IOutdoorHumidityService _outdoorHumidityService;

        public PersonalAgentService(
            SynonymService synonymService,
            IComponentService componentService,
            IAreaService areaService,
            IWeatherService weatherService,
            IOutdoorTemperatureService outdoorTemperatureService,
            IOutdoorHumidityService outdoorHumidityService)
        {
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (weatherService == null) throw new ArgumentNullException(nameof(weatherService));
            if (outdoorTemperatureService == null) throw new ArgumentNullException(nameof(outdoorTemperatureService));
            if (outdoorHumidityService == null) throw new ArgumentNullException(nameof(outdoorHumidityService));

            _synonymService = synonymService;
            _componentService = componentService;
            _areaService = areaService;
            _weatherService = weatherService;
            _outdoorTemperatureService = outdoorTemperatureService;
            _outdoorHumidityService = outdoorHumidityService;
        }

        public string ProcessMessage(IInboundMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var messageContextFactory = new MessageContextFactory(_synonymService, _areaService);
            var messageContext = messageContextFactory.Create(message);

            string answer;
            try
            {
                answer = ProcessMessage(messageContext);

                if (messageContext.GetContainsWord("debug"))
                {
                     answer += Environment.NewLine + Environment.NewLine + GenerateDebugOutput(messageContext);
                }
            }
            catch (Exception exception)
            {
                answer = $"{Emoji.Scream} Mist! Da ist etwas total schief gelaufen! Bitte stelle mir nie wieder solche Fragen!";
                Log.Error(exception, $"Error while processing message '{message.Text}'.");
            }

            return answer;
        }

        [ApiMethod]
        public void Ask(IApiContext apiContext)
        {
            var message = (string)apiContext.Request["Message"];
            if (string.IsNullOrEmpty(message))
            {
                apiContext.ResultCode = ApiResultCode.InvalidBody;
                return;
            }

            var inboundMessage = new ApiInboundMessage(DateTime.Now, message);
            var answer = ProcessMessage(inboundMessage);

            apiContext.Response["Answer"] = answer;
        }

        private string GenerateDebugOutput(MessageContext messageContext)
        {
            var debugOutput = new StringBuilder();

            debugOutput.AppendLine("<b>DEBUG:</b>");

            debugOutput.AppendLine("<b>[Original message]</b>");
            debugOutput.AppendLine(messageContext.OriginalMessage.Text);

            int counter = 1;
            debugOutput.AppendLine("<b>[Identified components]</b>");
            foreach (var componentId in messageContext.IdentifiedComponentIds)
            {
                debugOutput.AppendLine($"{counter} - {componentId}");
                counter++;
            }

            counter = 1;
            debugOutput.AppendLine("<b>[Identified areas]</b>");
            foreach (var areaId in messageContext.IdentifiedAreaIds)
            {
                debugOutput.AppendLine($"{counter} - {areaId}");
                counter++;
            }

            counter = 1;
            debugOutput.AppendLine("<b>[Filtered components]</b>");
            foreach (var componentId in messageContext.FilteredComponentIds)
            {
                debugOutput.AppendLine($"{counter} - {componentId}");
                counter++;
            }

            counter = 1;
            debugOutput.AppendLine("<b>[Identified component states]</b>");
            foreach (var componentState in messageContext.IdentifiedComponentStates)
            {
                debugOutput.AppendLine($"{counter} - {componentState}");
                counter++;
            }

            return debugOutput.ToString();
        }

        private string ProcessMessage(MessageContext messageContext)
        {
            if (messageContext.GetPatternMatch("Hi").Success)
            {
                return $"{Emoji.VictoryHand} Hi, was kann ich für Dich tun?";
            }

            if (messageContext.GetPatternMatch("Danke").Success)
            {
                return $"{Emoji.Wink} Habe ich doch gerne gemacht.";
            }

            if (messageContext.GetPatternMatch("Wetter").Success)
            {
                return GetWeatherStatus();
            }

            if (messageContext.GetPatternMatch("Fenster").Success)
            {
                return GetWindowStatus();
            }

            if (!messageContext.FilteredComponentIds.Any())
            {
                if (messageContext.IdentifiedComponentIds.Count > 0)
                {
                    return $"{Emoji.Confused} Mit so vielen Anfragen kann ich nicht umgehen. Bitte nenne mir nur eine eindeutige Komponente.";
                }

                return $"{Emoji.Confused} Du musst mir schon einen Sensor oder Aktor nennen.";
            }

            if (messageContext.FilteredComponentIds.Count > 1)
            {
                return $"{Emoji.Flushed} Bitte nicht mehrere Komponenten auf einmal.";
            }

            if (messageContext.FilteredComponentIds.Count == 1)
            {
                var component = _componentService.GetComponent<IComponent>(messageContext.IdentifiedComponentIds.First());

                IActuator actuator = component as IActuator;
                if (actuator != null)
                {
                    return UpdateActuatorState(actuator, messageContext);
                }

                ISensor sensor = component as ISensor;
                if (sensor != null)
                {
                    return GetSensorStatus(sensor);
                }
            }

            return $"{Emoji.Confused} Das habe ich leider nicht verstanden. Bitte stelle Deine Anfrage präziser.";
        }

        private string UpdateActuatorState(IActuator actuator, MessageContext messageContext)
        {
            if (messageContext.IdentifiedComponentStates.Count == 0)
            {
                return $"{Emoji.Confused} Was soll ich damit machen?";
            }

            if (messageContext.IdentifiedComponentStates.Count > 1)
            {
                return $"{Emoji.Confused} Das was du möchtest ist nicht eindeutig.";
            }

            if (!actuator.SupportsState(messageContext.IdentifiedComponentStates.First()))
            {
                return $"{Emoji.Hushed} Das wird nicht funktionieren.";
            }

            actuator.SetState(messageContext.IdentifiedComponentStates.First());
            return $"{Emoji.ThumbsUp} Habe ich erledigt. Kann ich noch etwas für dich tun?";
        }

        private string GetWeatherStatus()
        {
            var response = new StringBuilder();
            response.AppendLine($"{Emoji.BarChart} Das Wetter ist aktuell:");
            response.AppendLine($"Temperatur: {_outdoorTemperatureService.OutdoorTemperature}°C");
            response.AppendLine($"Luftfeuchtigkeit: {_outdoorHumidityService.OutdoorHumidity}%");
            response.AppendLine($"Wetter: {_weatherService.Weather}");

            return response.ToString();
        }

        private string GetWindowStatus()
        {
            var allWindows = _componentService.GetComponents<IWindow>();
            List<IWindow> openWindows = allWindows.Where(w => w.Casements.Any(c => !c.GetState().Equals(CasementStateId.Closed))).ToList();

            string response;
            if (!openWindows.Any())
            {
                response = $"{Emoji.Lock} Ich habe nachgesehen. Alle Fenster sind geschlossen.";
            }
            else
            {
                response = $"{Emoji.Unlock} Ich habe nachgesehen. Die folgenden Fenster sind noch (ganz oder auf Kipp) geöffnet:\r\n";
                response += string.Join(Environment.NewLine, openWindows.Select(w => "- " + w.Id));
            }

            return response;
        }

        private string GetSensorStatus(ISensor sensor)
        {
            var temperatureSensor = sensor as ITemperatureSensor;
            if (temperatureSensor != null)
            {
                return $"{Emoji.Fire} Die Temperatur dieses Sensor liegt aktuell bei {sensor.GetState()}°C";
            }

            var humiditySensor = sensor as IHumiditySensor;
            if (humiditySensor != null)
            {
                return $"{Emoji.SweatDrops} Die Luftfeuchtigkeit dieses Sensor liegt aktuell bei {sensor.GetState()}%";
            }

            return $"{Emoji.BarChart} Der sensor hat momentan den folgenden Zustand: {sensor.GetState()}";
        }
    }
}
