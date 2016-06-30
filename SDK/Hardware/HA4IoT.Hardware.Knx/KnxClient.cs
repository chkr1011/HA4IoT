using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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

        private bool _isConnected;
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

        public int Timeout { get; set; } = 100;

        public void Connect()
        {
            ThrowIfDisposed();

            Log.Verbose($"KnxClient: Connecting with {_hostName}...");

            var connectTask = _socket.ConnectAsync(_hostName, _port.ToString()).AsTask();
            connectTask.ConfigureAwait(false);
            if (!connectTask.Wait(Timeout))
            {
                throw new TimeoutException("Timeout while connecting KNX Client.");
            }

            _isConnected = true;

            Authenticate();

            Log.Verbose("KnxClient: Connected");
        }

        public void SendRequest(string request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            ThrowIfDisposed();
            ThrowIfNotConnected();

            byte[] payload = Encoding.UTF8.GetBytes(request + "\x03");
            WriteToSocket(payload);

            Log.Verbose($"KnxClient: Sent {request}");
        }

        public string SendRequestAndWaitForResponse(string request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            ThrowIfDisposed();
            ThrowIfNotConnected();

            byte[] payload = Encoding.UTF8.GetBytes(request + "\x03");
            WriteToSocket(payload);

            var buffer = ReadFromSocket();

            var response = Encoding.UTF8.GetString(buffer);
            Log.Verbose($"KnxClient: Received {response}");

            return response;
        }

        private void Authenticate()
        {
            Log.Verbose("KnxClient: Authenticating...");
            string response = SendRequestAndWaitForResponse($"p={_password}");

            ThrowIfNotAuthenticated(response);
        }

        private void ThrowIfNotAuthenticated(string response)
        {
            if (!response.Equals("p=ok\x03"))
            {
                throw new InvalidOperationException("Invalid password specified for KNX client.");
            }
        }

        private void ThrowIfNotConnected()
        {
            if (!_isConnected)
            {
                throw new InvalidOperationException("The KNX Client is not connected.");
            }
        }

        private void WriteToSocket(byte[] data)
        {
            var writeTask = _socket.OutputStream.WriteAsync(data.AsBuffer()).AsTask();
            writeTask.ConfigureAwait(false);
            if (!writeTask.Wait(Timeout))
            {
                throw new TimeoutException("Timeout while sending KNX Client request.");
            }
        }

        private byte[] ReadFromSocket()
        {
            Log.Verbose("KnxClient: Waiting for response...");

            var buffer = new Buffer(64);
            var readTask = _socket.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial).AsTask();
            readTask.ConfigureAwait(false);
            if (!readTask.Wait(Timeout))
            {
                throw new TimeoutException("Timeout while reading KNX Client response.");
            }

            return buffer.ToArray();
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
