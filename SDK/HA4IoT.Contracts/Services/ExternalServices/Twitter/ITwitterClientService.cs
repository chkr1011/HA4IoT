using System.Threading.Tasks;

namespace HA4IoT.Contracts.Services.ExternalServices.Twitter
{
    public interface ITwitterClientService : IService
    {
        Task Tweet(string message);
    }
}
