using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.WeatherStation;

namespace HA4IoT.Conditions.Specialized
{
    public class IsDayCondition : TimeRangeCondition
    {
        public IsDayCondition(IWeatherStation weatherStation, IHomeAutomationTimer timer) : base(timer)
        {
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));

            WithStart(() => weatherStation.Daylight.Sunrise);
            WithEnd(() => weatherStation.Daylight.Sunset);
        }
    }
}
