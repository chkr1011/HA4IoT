using System;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Tests.Mockups
{
    public class TestDateTimeService : ServiceBase, IDateTimeService 
    {
        public DateTime DateTime { get; set; }

        public void SetTime(TimeSpan time)
        {
            DateTime = new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, time.Hours, time.Minutes, time.Seconds);
        }

        public DateTime GetDate()
        {
            return DateTime.Date;
        }

        public TimeSpan GetTime()
        {
            return DateTime.TimeOfDay;
        }

        public DateTime GetDateTime()
        {
            return DateTime;
        }
    }
}
