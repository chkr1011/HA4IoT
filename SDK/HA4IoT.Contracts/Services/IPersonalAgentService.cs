using HA4IoT.Contracts.PersonalAgent;

namespace HA4IoT.Contracts.Services
{
    public interface IPersonalAgentService : IService
    {
        string ProcessMessage(IInboundMessage message);
    }
}