using System;

namespace HA4IoT.Contracts.Services.OutdoorHumidity
{
    public interface IOutdoorHumidityService : IApiExposedService
    {
        float OutdoorHumidity { get; }

        DateTime? Timestamp { get; }
    }
}
