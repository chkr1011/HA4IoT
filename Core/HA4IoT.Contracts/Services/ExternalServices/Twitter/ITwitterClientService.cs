using System.Threading.Tasks;

namespace HA4IoT.Contracts.Services.ExternalServices.Twitter
{
    public interface ITwitterClientService : IService
    {
        Task<bool> TryTweet(string message);
    }
}
