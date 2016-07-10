using System;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Core
{
    public class DateTimeService : ServiceBase, IDateTimeService
    {
        public DateTime GetDate()
        {
            return DateTime.Now.Date;
        }

        public TimeSpan GetTime()
        {
            return DateTime.Now.TimeOfDay;
        }

        public DateTime GetDateTime()
        {
            return DateTime.Now;
        }
    }
}
