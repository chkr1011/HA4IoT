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
        private const int Port = 19228;

        private readonly DatagramSocket _socket = new DatagramSocket();
        private readonly ISettingsService _settingsService;
        
        public DiscoveryServerService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            _socket.MessageReceived += SendResponseAsync;
        }

        public override void Startup()
        {
            _socket.BindServiceNameAsync(Port.ToString()).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }

        private async void SendResponseAsync(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            var controllerSettings = _settingsService.GetSettings<ControllerSettings>();

            var response = new DiscoveryResponse(controllerSettings.Caption, controllerSettings.Description);
            await SendResponseAsync(args.RemoteAddress, response);
        }

        private static async Task SendResponseAsync(HostName target, DiscoveryResponse response)
        {
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));

            using (var socket = new DatagramSocket())
            {
                await socket.ConnectAsync(target, Port.ToString());
                await socket.OutputStream.WriteAsync(buffer.AsBuffer());
                await socket.OutputStream.FlushAsync();
            }
        }
    }
}
