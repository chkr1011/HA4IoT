using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Core.Discovery;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking.Json;
using HA4IoT.Settings;

namespace HA4IoT.Core
{
    public sealed class DiscoveryServer : IDisposable
    {
        private int DEFAULT_PORT = 19228;

        private readonly ISettingsService _settingsService;
        
        private DatagramSocket _socket;

        public DiscoveryServer(ISettingsService settingsService, ISystemEventsService systemEventsService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (systemEventsService == null) throw new ArgumentNullException(nameof(systemEventsService));

            _settingsService = settingsService;
            systemEventsService.StartupCompleted += (s, e) => Bind();
        }

        public void Bind()
        {
            if (_socket != null)
            {
                throw new InvalidOperationException("The discovery server is already started.");
            }

            _socket = new DatagramSocket();
            _socket.MessageReceived += SendResponse;
            _socket.BindServiceNameAsync(DEFAULT_PORT.ToString()).AsTask().Wait();
        }

        private void SendResponse(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            var controllerSettings = _settingsService.GetSettings<ControllerSettings>();

            var response = new DiscoveryResponse(controllerSettings.Name, controllerSettings.Description);
            SendResponseAsync(args.RemoteAddress, response).Wait();
        }

        private async Task SendResponseAsync(HostName target, DiscoveryResponse response)
        {
            var buffer = response.ToJsonObject().ToByteArray();

            using (var socket = new DatagramSocket())
            {
                await socket.ConnectAsync(target, DEFAULT_PORT.ToString());
                await socket.OutputStream.WriteAsync(buffer.AsBuffer());
                await socket.OutputStream.FlushAsync();
            }
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }
    }
}
