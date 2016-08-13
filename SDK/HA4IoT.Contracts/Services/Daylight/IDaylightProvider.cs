using System;

namespace HA4IoT.Contracts.Services.Daylight
{
    public interface IDaylightProvider
    {
        event EventHandler<DaylightFetchedEventArgs> DaylightFetched;
    }
}
