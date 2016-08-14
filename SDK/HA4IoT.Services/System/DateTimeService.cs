using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking;

namespace HA4IoT.Services.System
{
    public class DateTimeService : ServiceBase, IDateTimeService
    {
        public DateTime Date => DateTime.Now.Date;

        public TimeSpan Time => DateTime.Now.TimeOfDay;
    
        public DateTime Now => DateTime.Now;
    
        public override void HandleApiRequest(IApiContext apiContext)
        {
            apiContext.Response.SetNamedDateTime("Date", Date);
            apiContext.Response.SetNamedTimeSpan("Time", Time);
            apiContext.Response.SetNamedDateTime("Now", Now);
        }
    }
}
