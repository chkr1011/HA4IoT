using System;

namespace HA4IoT.Contracts.Services.System
{
    public interface IDateTimeService : IService
    {
        DateTime GetDate();

        TimeSpan GetTime();

        DateTime GetDateTime();
    }
}
