using System;

namespace HA4IoT.Contracts.Services.System
{
    public interface IDateTimeService : IService
    {
        DateTime Date { get; }

        TimeSpan Time { get; }

        DateTime Now { get; }
    }
}
