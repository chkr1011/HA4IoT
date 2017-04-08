using System;

namespace HA4IoT.Contracts.Services.Daylight
{
    public interface IDaylightService : IService
    {
        TimeSpan Sunrise { get; }
        TimeSpan Sunset { get; }
        DateTime? Timestamp { get; }
        void Update(TimeSpan sunrise, TimeSpan sunset);
    }
}
