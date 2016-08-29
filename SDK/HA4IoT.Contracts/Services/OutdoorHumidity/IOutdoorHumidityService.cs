using System;

namespace HA4IoT.Contracts.Services.OutdoorHumidity
{
    public interface IOutdoorHumidityService : IService
    {
        float OutdoorHumidity { get; }
        DateTime? Timestamp { get; }
        void Update(float outdoorHumidity);
    }
}
