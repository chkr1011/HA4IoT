using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Core.Timer;

namespace HA4IoT.Actuators.Conditions.Specialized
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
