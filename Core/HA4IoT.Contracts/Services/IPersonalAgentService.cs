namespace HA4IoT.Contracts.Services
{
    public interface IPersonalAgentService : IService
    {
        string ProcessTextMessage(string message);
    }
}