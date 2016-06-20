using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace HA4IoT.Hardware.KNX
{
    public class WebServer
    {
        Socket _socket = null;
        static ManualResetEvent _clientDone = new ManualResetEvent(false);
        const int TIMEOUT_MILLISECONDS = 5000;
        const int MAX_BUFFER_SIZE = 2048;
        string port;

        public string Connect(string hostName, int portNumber)
        {
            this.port = portNumber.ToString();
            string result = string.Empty;
            DnsEndPoint hostEntry = new DnsEndPoint(hostName, portNumber);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.RemoteEndPoint = hostEntry;
            socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
            {
                result = e.SocketError.ToString();
                _clientDone.Set();
            });

            _clientDone.Reset();
            _socket.ConnectAsync(socketEventArg);
            _clientDone.WaitOne(TIMEOUT_MILLISECONDS);

            return result;
        }

        public string SendMessage(string data)
        {
            string response = "Operation Timeout";
            if (_socket != null)
            {
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
                socketEventArg.RemoteEndPoint = _socket.RemoteEndPoint;
                socketEventArg.UserToken = null;
                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
                {
                    response = e.SocketError.ToString();

                    _clientDone.Set();
                });

                byte[] payload = Encoding.UTF8.GetBytes(data);
                socketEventArg.SetBuffer(payload, 0, payload.Length);

                _clientDone.Reset();

                _socket.SendAsync(socketEventArg);

                _clientDone.WaitOne(TIMEOUT_MILLISECONDS);
            }
            else
            {
                response = "Socket is not initialized";
            }

            return response;
        }

        public string Receive()
        {
            string response = "Operation Timeout";

            // We are receiving over an established socket connection
            if (_socket != null)
            {
                // Create SocketAsyncEventArgs context object
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
                socketEventArg.RemoteEndPoint = _socket.RemoteEndPoint;

                // Setup the buffer to receive the data
                socketEventArg.SetBuffer(new Byte[MAX_BUFFER_SIZE], 0, MAX_BUFFER_SIZE);

                // Inline event handler for the Completed event.
                // Note: This even handler was implemented inline in order to make 
                // this method self-contained.
                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
                {
                    if (e.SocketError == System.Net.Sockets.SocketError.Success)
                    {
                        // Retrieve the data from the buffer
                        response = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                        response = response.Trim('\0');
                    }
                    else
                    {
                        response = e.SocketError.ToString();
                    }

                    _clientDone.Set();
                });

                // Sets the state of the event to nonsignaled, causing threads to block
                _clientDone.Reset();

                // Make an asynchronous Receive request over the socket
                _socket.ReceiveAsync(socketEventArg);

                // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
                // If no response comes back within this time then proceed
                _clientDone.WaitOne(TIMEOUT_MILLISECONDS);
            }
            else
            {
                response = "Socket is not initialized";
            }

            return response;
        }














        private const uint BufferSize = 8192;

        public async void ReceiveAsync()
        {
            StreamSocketListener listener = new StreamSocketListener();

            await listener.BindServiceNameAsync(port);

            listener.ConnectionReceived += async (sender, args) =>
            {
                StringBuilder request = new StringBuilder();
                using (IInputStream input = args.Socket.InputStream)
                {
                    byte[] data = new byte[BufferSize];
                    IBuffer buffer = data.AsBuffer();
                    uint dataRead = BufferSize;
                    while (dataRead == BufferSize)
                    {
                        await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                        request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                        dataRead = buffer.Length;
                    }
                }

                using (IOutputStream output = args.Socket.OutputStream)
                {
                    using (Stream response = output.AsStreamForWrite())
                    {
                        byte[] bodyArray = Encoding.UTF8.GetBytes("<html><body>Hello, World!</body></html>");
                        var bodyStream = new MemoryStream(bodyArray);

                        var header = "HTTP/1.1 200 OK\r\n" +
                                    $"Content-Length: {bodyStream.Length}\r\n" +
                                        "Connection: close\r\n\r\n";

                        byte[] headerArray = Encoding.UTF8.GetBytes(header);
                        await response.WriteAsync(headerArray, 0, headerArray.Length);
                        await bodyStream.CopyToAsync(response);
                        await response.FlushAsync();
                    }
                }
            };
        }
    }
}
