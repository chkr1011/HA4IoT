using System;

namespace HA4IoT.Contracts.Services.OutdoorTemperature
{
    public interface IOutdoorTemperatureService: IService
    {
        float OutdoorTemperature { get; }
        DateTime? Timestamp { get; }
        void Update(float outdoorTemperature);
    }
}
