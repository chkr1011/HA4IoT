using System;

namespace HA4IoT.Contracts.Services
{
    public interface IDaylightService : IService
    {
        TimeSpan GetSunrise();

        TimeSpan GetSunset();
    }
}
