using System.Threading.Tasks;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.ExternalServices.Twitter
{
    public interface ITwitterClientService : IService
    {
        Task<bool> TryTweet(string message);
    }
}
