using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.PersonalAgent;
using HA4IoT.Contracts.PersonalAgent.AmazonEcho;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.Weather;
using Newtonsoft.Json.Linq;

namespace HA4IoT.PersonalAgent
{
    [ApiServiceClass(typeof(IPersonalAgentService))]
    public class PersonalAgentService : ServiceBase, IPersonalAgentService
    {
        private readonly ISettingsService _settingsService;
        private readonly IComponentRegistryService _componentsRegistry;
        private readonly IAreaRegistryService _areaService;
        private readonly IWeatherService _weatherService;
        private readonly IOutdoorTemperatureService _outdoorTemperatureService;
        private readonly IOutdoorHumidityService _outdoorHumidityService;
        private readonly ILogger _log;

        private MessageContext _latestMessageContext;

        public PersonalAgentService(
            ISettingsService settingsService,
            IComponentRegistryService componentRegistry,
            IAreaRegistryService areaService,
            IWeatherService weatherService,
            IOutdoorTemperatureService outdoorTemperatureService,
            IOutdoorHumidityService outdoorHumidityService,
            ILogService logService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _componentsRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            _outdoorTemperatureService = outdoorTemperatureService ?? throw new ArgumentNullException(nameof(outdoorTemperatureService));
            _outdoorHumidityService = outdoorHumidityService ?? throw new ArgumentNullException(nameof(outdoorHumidityService));
            _log = logService?.CreatePublisher(nameof(PersonalAgentService)) ?? throw new ArgumentNullException(nameof(logService));
        }

        [ApiMethod]
        public void ProcessSkillServiceRequest(IApiCall apiCall)
        {
            var request = apiCall.Parameter.ToObject<SkillServiceRequest>();

            var messageContextFactory = new MessageContextFactory(_areaService, _componentsRegistry, _settingsService);
            var messageContext = messageContextFactory.Create(request);

            ProcessMessage(messageContext);

            var response = new SkillServiceResponse();
            response.Response.OutputSpeech.Text = messageContext.Reply;

            apiCall.Result = JObject.FromObject(response);
        }

        [ApiMethod]
        public void Ask(IApiCall apiCall)
        {
            var text = (string)apiCall.Parameter["Message"];
            if (string.IsNullOrEmpty(text))
            {
                apiCall.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }

            apiCall.Result["Answer"] = ProcessTextMessage(text);
        }

        public string ProcessTextMessage(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var messageContextFactory = new MessageContextFactory(_areaService, _componentsRegistry, _settingsService);
            var messageContext = messageContextFactory.Create(text);

            ProcessMessage(messageContext);
            return messageContext.Reply;
        }

        [ApiMethod]
        public void GetLatestMessageContext(IApiCall apiCall)
        {
            if (_latestMessageContext == null)
            {
                return;
            }

            apiCall.Result = JObject.FromObject(_latestMessageContext);
        }

        private void ProcessMessage(MessageContext messageContext)
        {
            try
            {
                _latestMessageContext = messageContext;
                messageContext.Reply = ProcessMessageInternal(messageContext);
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Error while processing personal agent message '{messageContext.Text}'.");
            }
        }

        private string ProcessMessageInternal(MessageContext messageContext)
        {
            if (GetPatternMatch(messageContext, "Hi").Success)
            {
                if (messageContext.Kind == MessageContextKind.Speech)
                {
                    return "Hi, was kann ich für Dich tun?";
                }

                return $"{Emoji.VictoryHand} Hi, was kann ich für Dich tun?";
            }

            if (GetPatternMatch(messageContext, "Danke").Success)
            {
                return $"{Emoji.Wink} Habe ich doch gerne gemacht.";
            }

            if (GetPatternMatch(messageContext, "Wetter").Success)
            {
                return GetWeatherStatus();
            }

            if (GetPatternMatch(messageContext, "Fenster").Success)
            {
                return GetWindowStatus();
            }

            if (!messageContext.AffectedComponentIds.Any())
            {
                if (messageContext.IdentifiedComponentIds.Count > 0)
                {
                    if (messageContext.Kind == MessageContextKind.Speech)
                    {
                        return "Auf deine Beschreibung passen mehrere Geräte. Bitte stelle deine Anfrage präziser.";
                    }

                    return $"{Emoji.Confused} Auf deine Beschreibung passen mehrere Geräte. Bitte stelle deine Anfrage präziser.";
                }

                if (messageContext.Kind == MessageContextKind.Speech)
                {
                    return "Ich konnte kein Gerät finden, dass auf deine Beschreibung passt. Bitte stelle deine Anfrage präziser.";
                }

                return $"{Emoji.Confused} Ich konnte kein Gerät finden, dass auf deine Beschreibung passt. Bitte stelle deine Anfrage präziser.";
            }

            if (messageContext.AffectedComponentIds.Count > 1)
            {
                return $"{Emoji.Flushed} Bitte nicht mehrere Geräte auf einmal.";
            }

            if (messageContext.AffectedComponentIds.Count == 1)
            {
                var component = _componentsRegistry.GetComponent<IComponent>(messageContext.AffectedComponentIds.First());

                var temperatureSensor = component as ITemperatureSensor;
                if (temperatureSensor != null)
                {
                    return $"{Emoji.Fire} Die Temperatur dieses Sensor liegt aktuell bei {component.GetState()}°C";
                }

                var humiditySensor = component as IHumiditySensor;
                if (humiditySensor != null)
                {
                    return $"{Emoji.SweatDrops} Die Luftfeuchtigkeit dieses Sensor liegt aktuell bei {component.GetState()}%";
                }

                return InvokeCommand(component, messageContext);
            }

            return $"{Emoji.Confused} Das habe ich leider nicht verstanden. Bitte stelle Deine Anfrage präziser.";
        }

        public Match GetPatternMatch(MessageContext messageContext, string pattern)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            return Regex.Match(messageContext.Text ?? string.Empty, pattern, RegexOptions.IgnoreCase);
        }

        private string InvokeCommand(IComponent component, MessageContext messageContext)
        {
            if (messageContext.IdentifiedCommands.Count == 0)
            {
                return $"{Emoji.Confused} Was soll ich damit machen?";
            }

            if (messageContext.IdentifiedCommands.Count > 1)
            {
                return $"{Emoji.Confused} Das was du möchtest ist nicht eindeutig.";
            }

            try
            {
                component.ExecuteCommand(messageContext.IdentifiedCommands.First());
            }
            catch (CommandNotSupportedException)
            {
                return $"{Emoji.Confused} Das was du möchtest hat nicht funktioniert.";
            }

            if (messageContext.Kind == MessageContextKind.Speech)
            {
                return "Habe ich erledigt.";
            }

            return $"{Emoji.ThumbsUp} Habe ich erledigt.";
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
            var allWindows = _componentsRegistry.GetComponents<IWindow>();
            var openWindows = allWindows.Where(w => !w.GetState().Has(WindowState.Closed)).ToList();

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
    }
}
