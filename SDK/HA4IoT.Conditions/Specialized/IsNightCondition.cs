using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.WeatherStation;

namespace HA4IoT.Conditions.Specialized
{
    public class IsNightCondition : TimeRangeCondition
    {
        public IsNightCondition(IWeatherStation weatherStation, IHomeAutomationTimer timer) : base(timer)
        {
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));

            WithStart(() => weatherStation.Daylight.Sunset);
            WithEnd(() => weatherStation.Daylight.Sunrise);
        }
    }
}
