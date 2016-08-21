using System.Threading.Tasks;

namespace HA4IoT.Contracts.Networking
{
    public interface IWebSocketClientSession
    {
        Task SendAsync(byte[] data);
    }
}
