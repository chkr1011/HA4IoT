using System;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Environment
{
    public interface IDaylightService : IService
    {
        TimeSpan Sunrise { get; }
        TimeSpan Sunset { get; }
        DateTime? Timestamp { get; }
        void Update(TimeSpan sunrise, TimeSpan sunset);
    }
}
