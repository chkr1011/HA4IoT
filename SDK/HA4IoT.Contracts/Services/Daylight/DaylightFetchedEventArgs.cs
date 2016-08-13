using System;

namespace HA4IoT.Contracts.Services.Daylight
{
    public class DaylightFetchedEventArgs : EventArgs
    {
        public DaylightFetchedEventArgs(TimeSpan sunrise, TimeSpan sunset)
        {
            Sunrise = sunrise;
            Sunset = sunset;
        }

        public TimeSpan Sunrise { get; }

        public TimeSpan Sunset { get; }
    }
}
