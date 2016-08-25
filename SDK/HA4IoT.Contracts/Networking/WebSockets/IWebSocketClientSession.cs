using System;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace HA4IoT.Contracts.Networking.WebSockets
{
    public interface IWebSocketClientSession
    {
        event EventHandler Closed;

        event EventHandler<WebSocketMessageReceivedEventArgs> MessageReceived;

        Task SendAsync(JsonObject json);

        Task SendAsync(string text);

        Task SendAsync(byte[] data);

        Task CloseAsync();
    }
}
