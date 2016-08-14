using System;

namespace HA4IoT.Contracts.Services.Daylight
{
    public interface IDaylightService
    {
        TimeSpan Sunrise { get; }

        TimeSpan Sunset { get; }

        DateTime? Timestamp { get; }
    }
}
