using System;

namespace HA4IoT.Contracts.Services
{
    public interface IDaylightService : IService
    {
        TimeSpan Sunrise { get; }

        TimeSpan Sunset { get; }
    }
}
