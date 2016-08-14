using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.PersonalAgent;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.Weather;

namespace HA4IoT.PersonalAgent
{
    public class PersonalAgentService
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
            var messageProcessor = new PersonalAgentMessageProcessor(_synonymService, _componentService, _areaService, _weatherService, _outdoorTemperatureService, _outdoorHumidityService);
            messageProcessor.ProcessMessage(message);

            return messageProcessor.Answer;
        }
    }
}
