using System;

namespace HA4IoT.Contracts.Services.OutdoorTemperature
{
    public interface IOutdoorTemperatureProvider
    {
        event EventHandler<OutdoorTemperatureFetchedEventArgs> OutdoorTemperatureFetched;
    }
}
