using System;

namespace HA4IoT.Contracts.Services.OutdoorTemperature
{
    public interface IOutdoorTemperatureService : IApiExposedService
    {
        float OutdoorTemperature { get; }

        DateTime? Timestamp { get; }
    }
}
