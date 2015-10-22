using System;

namespace HA4IoT.Actuators
{
    public struct Daylight
    {
        public Daylight(TimeSpan sunrise, TimeSpan sunset)
        {
            Sunrise = sunrise;
            Sunset = sunset;

            if (Sunrise == TimeSpan.Zero || Sunset == TimeSpan.Zero)
            {
                IsDay = true;
            }
            else
            {
                TimeSpan time = DateTime.Now.TimeOfDay;
                IsDay = time > Sunrise && time < Sunset;
            }

            IsNight = !IsDay;
        }

        public TimeSpan Sunset { get; }

        public TimeSpan Sunrise { get; }

        public bool IsDay { get; }

        public bool IsNight { get; }

        public Daylight Move(TimeSpan sunriseDiff, TimeSpan sunsetDiff)
        {
            return new Daylight(Sunrise.Add(sunriseDiff), Sunset.Add(sunsetDiff));
        }
    }
}
