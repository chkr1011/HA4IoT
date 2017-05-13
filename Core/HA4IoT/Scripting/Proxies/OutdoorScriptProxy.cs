using System;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.System;
using MoonSharp.Interpreter;

namespace HA4IoT.Scripting.Proxies
{
    public class OutdoorScriptProxy : IScriptProxy
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IOutdoorTemperatureService _outdoorTemperatureService;
        private readonly IOutdoorHumidityService _outdoorHumidityService;
        private readonly IDaylightService _daylightService;

        [MoonSharpHidden]
        public OutdoorScriptProxy(IOutdoorTemperatureService outdoorTemperatureService, IOutdoorHumidityService outdoorHumidityService, IDaylightService daylightService, IDateTimeService dateTimeService)
        {
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _outdoorTemperatureService = outdoorTemperatureService ?? throw new ArgumentNullException(nameof(outdoorTemperatureService));
            _outdoorHumidityService = outdoorHumidityService ?? throw new ArgumentNullException(nameof(outdoorHumidityService));
            _daylightService = daylightService ?? throw new ArgumentNullException(nameof(daylightService));
        }

        [MoonSharpHidden]
        public string Name => "outdoor";

        public float? GetTemperature()
        {
            return _outdoorTemperatureService.OutdoorTemperature;
        }

        public float? GetHumidity()
        {
            return _outdoorHumidityService.OutdoorHumidity;
        }

        public string GetSunrise()
        {
            return _daylightService.Sunrise.ToString("c");
        }

        public string GetSunset()
        {
            return _daylightService.Sunset.ToString("c");
        }

        public bool GetIsDay()
        {
            var isDayCondition = new IsDayCondition(_daylightService, _dateTimeService);
            return isDayCondition.IsFulfilled();
        }

        public bool GetIsNight()
        {
            var isDayCondition = new IsDayCondition(_daylightService, _dateTimeService);
            return !isDayCondition.IsFulfilled();
        }
    }
}
