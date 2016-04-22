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
        private readonly List<ComponentId> _filteredComponentIds = new List<ComponentId>();
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
            var messageContextFactory = new MessageContextFactory(synonymService);
            _messageContext = messageContextFactory.Create(message);

            try
            {
                Answer = ProcessMessage();
            }
            catch (Exception exception)
            {
                Answer = $"{Emoji.Scream} Mist! Da ist etwas total schief gelaufen! Bitte stelle mir nie wieder solche Fragen!";
                Log.Error(exception, $"Error while processing message '{message.Text}'.");              
            }
        }

        private string ProcessMessage()
        {
            if (_messageContext.GetPatternMatch("Hi").Success)
            {
                return $"{Emoji.VictoryHand} Hi, was kann ich für Dich tun?";
            }

            if (_messageContext.GetPatternMatch("Danke").Success)
            {
                return $"{Emoji.Wink} Gerne.";                
            }

            if (_messageContext.GetPatternMatch("Wetter").Success)
            {
                return GetWeatherStatus();
            }

            if (_messageContext.GetPatternMatch("Fenster").Success)
            {
                return GetWindowStatus();
            }

            FilterComponentIds();

            if (!_filteredComponentIds.Any())
            {
                return $"{Emoji.Confused} Du musst mir schon einen Sensor oder Aktor nennen.";
            }
            
            if (_filteredComponentIds.Count > 1)
            {
                return $"{Emoji.Flushed} Bitte nicht mehrere Komponenten auf einmal.";
            }

            if (_filteredComponentIds.Count == 1)
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

        private void FilterComponentIds()
        {
            if (!_messageContext.IdentifiedComponentIds.Any())
            {
                return;
            }

            if (_messageContext.IdentifiedComponentIds.Count == 1)
            {
                _filteredComponentIds.Add(_messageContext.IdentifiedComponentIds.First());
                return;
            }

            foreach (var componentId in _messageContext.IdentifiedComponentIds)
            {
                foreach (var areaId in _messageContext.IdentifiedAreaIds)
                {
                    var area = _controller.GetArea(areaId);

                    if (area.GetContainsComponent(componentId))
                    {
                        _filteredComponentIds.Add(componentId);
                        break;
                    }
                }
            }
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

            if (!actuator.GetSupportsState(_messageContext.IdentifiedComponentStates.First()))
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
