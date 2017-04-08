using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Core.Discovery;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Settings;
using Newtonsoft.Json;

namespace HA4IoT.Services.System
{
    public sealed class DiscoveryServerService : ServiceBase, IDisposable
    {
        private int DEFAULT_PORT = 19228;

        private readonly ISettingsService _settingsService;
        private readonly DatagramSocket _socket = new DatagramSocket();

        public DiscoveryServerService(ISettingsService settingsService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            
            _settingsService = settingsService;

            _socket.MessageReceived += SendResponse;
        }

        public override void Startup()
        {
            _socket.BindServiceNameAsync(DEFAULT_PORT.ToString()).AsTask().Wait();
        }

        private void SendResponse(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            var controllerSettings = _settingsService.GetSettings<ControllerSettings>();

            var response = new DiscoveryResponse(controllerSettings.Caption, controllerSettings.Description);
            SendResponseAsync(args.RemoteAddress, response).Wait();
        }

        private async Task SendResponseAsync(HostName target, DiscoveryResponse response)
        {
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));

            using (var socket = new DatagramSocket())
            {
                await socket.ConnectAsync(target, DEFAULT_PORT.ToString());
                await socket.OutputStream.WriteAsync(buffer.AsBuffer());
                await socket.OutputStream.FlushAsync();
            }
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
