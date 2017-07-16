using System;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Core
{
    public interface IDateTimeService : IService
    {
        DateTime Date { get; }

        TimeSpan Time { get; }

        DateTime Now { get; }
    }
}
