using System;

namespace HA4IoT.Contracts.Services.OutdoorHumidity
{
    public interface IOutdoorHumidityProvider
    {
        event EventHandler<OutdoorHumidityFetchedEventArgs> OutdoorHumidityFetched;
    }
}
