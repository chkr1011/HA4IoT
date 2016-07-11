using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking;

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

        public override void HandleApiRequest(IApiContext apiContext)
        {
            apiContext.Response.SetNamedDateTime("Date", GetDate());
            apiContext.Response.SetNamedTimeSpan("Time", GetTime());
            apiContext.Response.SetNamedDateTime("DateTime", GetDateTime());
        }
    }
}
