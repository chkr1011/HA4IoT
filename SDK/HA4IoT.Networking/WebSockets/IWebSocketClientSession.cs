using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Networking.WebSockets
{
    public interface IWebSocketClientSession
    {
        event EventHandler Closed;

        event EventHandler<WebSocketMessageReceivedEventArgs> MessageReceived;

        Task SendAsync(JObject json);

        Task SendAsync(string text);

        Task SendAsync(byte[] data);

        Task CloseAsync();
    }
}
