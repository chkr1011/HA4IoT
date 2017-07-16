using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.PersonalAgent
{
    public interface IPersonalAgentService : IService
    {
        string ProcessTextMessage(string message);
    }
}