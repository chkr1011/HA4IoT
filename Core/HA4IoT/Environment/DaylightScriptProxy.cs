using System;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Environment;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services;
using MoonSharp.Interpreter;

namespace HA4IoT.Environment
{
    public class DaylightScriptProxy : IScriptProxy
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IDaylightService _daylightService;

        [MoonSharpHidden]
        public DaylightScriptProxy(IDaylightService daylightService, IDateTimeService dateTimeService)
        {
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _daylightService = daylightService ?? throw new ArgumentNullException(nameof(daylightService));
        }

        [MoonSharpHidden]
        public string Name => "daylight";

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
