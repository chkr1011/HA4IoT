using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Logging
{
    public interface ILogService : IService
    {
        ILogger CreatePublisher(string source);
    }
}
