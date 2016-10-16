using System;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Conditions.Specialized
{
    public class IsNightCondition : TimeRangeCondition
    {
        public IsNightCondition(IDaylightService daylightService, IDateTimeService dateTimeService) 
            : base(dateTimeService)
        {
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));

            WithStart(daylightService.Sunset);
            WithEnd(daylightService.Sunrise);
        }
    }
}
