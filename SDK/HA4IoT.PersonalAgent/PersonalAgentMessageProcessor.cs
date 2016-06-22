using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.PersonalAgent;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.WeatherService;
using HA4IoT.Components;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.PersonalAgent
{
    public class PersonalAgentMessageProcessor
    {
        private readonly IController _controller;
        
        private MessageContext _messageContext;

        public PersonalAgentMessageProcessor(IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            _controller = controller;
        }

        public string Answer { get; private set; }

        public void ProcessMessage(IInboundMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var synonymService = _controller.GetService<SynonymService>();
            var messageContextFactory = new MessageContextFactory(synonymService, _controller);
            _messageContext = messageContextFactory.Create(message);

            try
            {
                Answer = ProcessMessage();

                if (_messageContext.GetContainsWord("debug"))
                {
                    Answer += Environment.NewLine + Environment.NewLine + GenerateDebugOutput();
                }
            }
            catch (Exception exception)
            {
                Answer = $"{Emoji.Scream} Mist! Da ist etwas total schief gelaufen! Bitte stelle mir nie wieder solche Fragen!";
                Log.Error(exception, $"Error while processing message '{message.Text}'.");              
            }
        }

        private string GenerateDebugOutput()
        {
            var debugOutput = new StringBuilder();

            debugOutput.AppendLine("<b>DEBUG:</b>");

            debugOutput.AppendLine("<b>[Original message]</b>");
            debugOutput.AppendLine(_messageContext.OriginalMessage.Text);

            int counter = 1;
            debugOutput.AppendLine("<b>[Identified components]</b>");
            foreach (var componentId in _messageContext.IdentifiedComponentIds)
            {
                debugOutput.AppendLine($"{counter} - {componentId}");
                counter++;
            }

            counter = 1;
            debugOutput.AppendLine("<b>[Identified areas]</b>");
            foreach (var areaId in _messageContext.IdentifiedAreaIds)
            {
                debugOutput.AppendLine($"{counter} - {areaId}");
                counter++;
            }

            counter = 1;
            debugOutput.AppendLine("<b>[Filtered components]</b>");
            foreach (var componentId in _messageContext.FilteredComponentIds)
            {
                debugOutput.AppendLine($"{counter} - {componentId}");
                counter++;
            }

            counter = 1;
            debugOutput.AppendLine("<b>[Identified component states]</b>");
            foreach (var componentState in _messageContext.IdentifiedComponentStates)
            {
                debugOutput.AppendLine($"{counter} - {componentState}");
                counter ++;
            }

            return debugOutput.ToString();
        }

        private string ProcessMessage()
        {
            if (_messageContext.GetPatternMatch("Hi").Success)
            {
                return $"{Emoji.VictoryHand} Hi, was kann ich für Dich tun?";
            }

            if (_messageContext.GetPatternMatch("Danke").Success)
            {
                return $"{Emoji.Wink} Habe ich doch gerne gemacht.";                
            }

            if (_messageContext.GetPatternMatch("Wetter").Success)
            {
                return GetWeatherStatus();
            }

            if (_messageContext.GetPatternMatch("Fenster").Success)
            {
                return GetWindowStatus();
            }

            if (!_messageContext.FilteredComponentIds.Any())
            {
                if (_messageContext.IdentifiedComponentIds.Count > 0)
                {
                    return $"{Emoji.Confused} Mit so vielen Anfragen kann ich nicht umgehen. Bitte nenne mir nur eine eindeutige Komponente.";
                }

                return $"{Emoji.Confused} Du musst mir schon einen Sensor oder Aktor nennen.";
            }
            
            if (_messageContext.FilteredComponentIds.Count > 1)
            {
                return $"{Emoji.Flushed} Bitte nicht mehrere Komponenten auf einmal.";
            }

            if (_messageContext.FilteredComponentIds.Count == 1)
            {
                var component = _controller.GetComponent<IComponent>(_messageContext.IdentifiedComponentIds.First());

                IActuator actuator = component as IActuator;
                if (actuator != null)
                {
                    return UpdateActuatorState(actuator);
                }

                ISensor sensor = component as ISensor;
                if (sensor != null)
                {
                    return GetSensorStatus(sensor);
                }
            }
            
            return $"{Emoji.Confused} Das habe ich leider nicht verstanden. Bitte stelle Deine Anfrage präziser.";
        }

        private string UpdateActuatorState(IActuator actuator)
        {
            if (_messageContext.IdentifiedComponentStates.Count == 0)
            {
                return $"{Emoji.Confused} Was soll ich damit machen?";
            }

            if (_messageContext.IdentifiedComponentStates.Count > 1)
            {
                return $"{Emoji.Confused} Das was du möchtest ist nicht eindeutig.";
            }

            if (!actuator.SupportsState(_messageContext.IdentifiedComponentStates.First()))
            {
                return $"{Emoji.Hushed} Das wird nicht funktionieren.";
            }

            actuator.SetState(_messageContext.IdentifiedComponentStates.First());
            return $"{Emoji.ThumbsUp} Habe ich erledigt. Kann ich noch etwas für dich tun?";
        }

        private string GetWeatherStatus()
        {
            var weatherService = _controller.GetService<IWeatherService>();

            var response = new StringBuilder();
            response.AppendLine($"{Emoji.BarChart} Das Wetter ist aktuell:");
            response.AppendLine($"Temperatur: {weatherService.GetTemperature()}°C");
            response.AppendLine($"Luftfeuchtigkeit: {weatherService.GetHumidity()}%");
            response.AppendLine($"Situation: {weatherService.GetSituation()}");

            return response.ToString();
        }

        private string GetWindowStatus()
        {
            var allWindows = _controller.GetComponents<IWindow>();
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
