using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking;
using Windows.Networking.Sockets;
using HA4IoT.Contracts.Core.Discovery;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking.Json;
using HA4IoT.Services.System;
using HA4IoT.Settings;

namespace HA4IoT.Core
{
    public sealed class DiscoveryServer : IDisposable
    {
        private int DEFAULT_PORT = 19228;

        private readonly ControllerSettings _controllerSettings;

        private DatagramSocket _socket;

        public DiscoveryServer(ControllerSettings controllerSettings, ISystemEventsService systemEventsService)
        {
            if (controllerSettings == null) throw new ArgumentNullException(nameof(controllerSettings));
            if (systemEventsService == null) throw new ArgumentNullException(nameof(systemEventsService));

            _controllerSettings = controllerSettings;

            systemEventsService.StartupCompleted += (s, e) => Start();
        }

        public void Start()
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
            var response = new DiscoveryResponse(_controllerSettings.Name, _controllerSettings.Description);
            SendResponseAsync(args.RemoteAddress, response).Wait();
        }

        private async Task SendResponseAsync(HostName target, DiscoveryResponse response)
        {
            using (var socket = new DatagramSocket())
            {
                await socket.ConnectAsync(target, DEFAULT_PORT.ToString());

                using (Stream outputStream = socket.OutputStream.AsStreamForWrite())
                {
                    byte[] buffer = SerializeResponse(response);
                    outputStream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        private byte[] SerializeResponse(DiscoveryResponse response)
        {
            var controllerObject = new JsonObject();
            controllerObject.SetNamedValue("Caption", response.ControllerCaption.ToJsonValue());
            controllerObject.SetNamedValue("Description", response.ControllerDescription.ToJsonValue());

            var result = new JsonObject();
            result.SetNamedValue("Type", "HA4IoT.DiscoveryResponse".ToJsonValue());
            result.SetNamedValue("Version", 1.ToJsonValue());
            result.SetNamedValue("Controller", controllerObject);

            return result.ToByteArray();
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }
    }
}
