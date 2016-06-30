using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using HA4IoT.Contracts.Logging;
using Buffer = Windows.Storage.Streams.Buffer;

namespace HA4IoT.Hardware.Knx
{
    public sealed class KnxClient : IDisposable
    {
        private readonly StreamSocket _socket = new StreamSocket();
        private readonly HostName _hostName;
        private readonly int _port;
        private readonly string _password;

        private bool _isDisposed;
        
        public KnxClient(HostName hostName, int port, string password)
        {
            if (hostName == null) throw new ArgumentNullException(nameof(hostName));

            _hostName = hostName;
            _port = port;
            _password = password;

            _socket.Control.KeepAlive = true;
            _socket.Control.NoDelay = true;
        }

        public async Task Connect()
        {
            ThrowIfDisposed();

            Log.Verbose($"KnxClient: Connecting with {_hostName}...");
            await _socket.ConnectAsync(_hostName, _port.ToString());

            await Authenticate();

            Log.Verbose("KnxClient: Connected");
        }

        public async Task SendCommand(string command)
        {
            ThrowIfDisposed();

            byte[] payload = Encoding.UTF8.GetBytes(command + "\x03");
            await _socket.OutputStream.WriteAsync(payload.AsBuffer());

            Log.Verbose($"KnxClient: Sent {command}");
        }

        public async Task<string> SendRequestAndWaitForResponse(string request)
        {
            await SendCommand(request);

            var buffer = new Buffer(64);
            await _socket.InputStream.ReadAsync(buffer, buffer.Length, InputStreamOptions.Partial);

            var response = Encoding.UTF8.GetString(buffer.ToArray());
            Log.Verbose($"KnxClient: Received {response}");

            return response;
        }

        private async Task Authenticate()
        {
            Log.Verbose("KnxClient: Authenticating...");
            await _socket.OutputStream.WriteAsync(GeneratePayload($"p={_password}\x03"));

            Log.Verbose("KnxClient: Waiting for response...");
            var buffer = new Buffer(16);
            await _socket.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial);

            ThrowIfNotAuthenticated(buffer);
        }

        private void ThrowIfNotAuthenticated(Buffer buffer)
        {
            string response = Encoding.UTF8.GetString(buffer.ToArray());
            if (!response.Equals("p=ok\x03"))
            {
                throw new InvalidOperationException("Invalid password specified for KNX client.");
            }
        }

        private IBuffer GeneratePayload(string command)
        {
            byte[] data = Encoding.UTF8.GetBytes(command);
            return data.AsBuffer();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new InvalidOperationException("The KNX client is already disposed.");
        }

        public void Dispose()
        {
            _isDisposed = true;
            _socket.Dispose();
        }   
    }
}
