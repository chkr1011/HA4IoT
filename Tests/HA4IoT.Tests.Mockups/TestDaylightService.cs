using System;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Tests.Mockups
{
    public class TestDaylightService : ServiceBase, IDaylightService
    {
        public TestDaylightService()
        {
            Sunrise = TimeSpan.Parse("06:00");
            Sunset = TimeSpan.Parse("18:00");
        }

        public TimeSpan Sunrise { get; set; }
        public TimeSpan Sunset { get; set; }

        public TimeSpan GetSunrise()
        {
            return Sunrise;
        }

        public TimeSpan GetSunset()
        {
            return Sunset;
        }
    }
}
