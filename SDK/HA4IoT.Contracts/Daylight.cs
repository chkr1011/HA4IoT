using System;

namespace HA4IoT.Contracts
{
    public class Daylight
    {
        public Daylight(TimeSpan timeOfDay, TimeSpan sunrise, TimeSpan sunset)
        {
            Sunrise = sunrise;
            Sunset = sunset;

            if (Sunrise == TimeSpan.Zero || Sunset == TimeSpan.Zero)
            {
                IsDay = true;
            }
            else
            {
                IsDay = timeOfDay > Sunrise && timeOfDay < Sunset;
            }

            IsNight = !IsDay;
        }

        public TimeSpan Sunset { get; }

        public TimeSpan Sunrise { get; }

        public bool IsDay { get; }

        public bool IsNight { get; }
    }
}
