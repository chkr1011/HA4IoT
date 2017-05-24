using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Logging
{
    public interface ILogService : IService
    {
        int WarningsCount { get; }
        int ErrorsCount { get; }
        
        ILogger CreatePublisher(string source);
    }
}
