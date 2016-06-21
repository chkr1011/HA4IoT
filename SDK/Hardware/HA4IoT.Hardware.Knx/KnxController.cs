using HA4IoT.Contracts.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HA4IoT.Contracts.Api;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace HA4IoT.Hardware.Knx
{
    public class KnxController
    {
        //var knxController = new KnxController("192.168.1.123","8150");
        //var lamp = new HA4IoT.Actuators.Lamp(new ComponentId("Lamp1"), knxController.CreateDigitalJoinEndpoint("d1"));

        Socket _socket = null;
        static ManualResetEvent _clientDone = new ManualResetEvent(false);
        const int TIMEOUT_MILLISECONDS = 5000;
        const int MAX_BUFFER_SIZE = 2048;
        int port;

        public KnxController(string ip,string port)
        {
            try
            {
                this.port = Convert.ToInt32(port);
                DnsEndPoint hostEntry = new DnsEndPoint(ip, this.port);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


            ToggleDigitalOutput("D20");
        }

        public void CreateDigitalJoinEndpoint(string digitalJoin)
        {

        }

        public void ToggleDigitalOutput(string id)
        {
            if (_socket != null)
            {
                // Create SocketAsyncEventArgs context object
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

                // Set properties on context object
                socketEventArg.RemoteEndPoint = _socket.RemoteEndPoint;
                socketEventArg.UserToken = null;

                // Inline event handler for the Completed event.
                // Note: This event handler was implemented inline in order 
                // to make this method self-contained.
                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
                {
                    // Unblock the UI thread
                    _clientDone.Set();
                });

                // Add the data to be sent into the buffer
                byte[] payload = Encoding.UTF8.GetBytes(id + "=1\x03");
                socketEventArg.SetBuffer(payload, 0, payload.Length);

                // Sets the state of the event to nonsignaled, causing threads to block
                _clientDone.Reset();

                // Make an asynchronous Send request over the socket
                _socket.SendAsync(socketEventArg);



                // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
                // If no response comes back within this time then proceed
                _clientDone.WaitOne(TIMEOUT_MILLISECONDS);

                // Add the data to be sent into the buffer
                payload = Encoding.UTF8.GetBytes(id + "=0\x03");
                socketEventArg.SetBuffer(payload, 0, payload.Length);

                // Sets the state of the event to nonsignaled, causing threads to block
                _clientDone.Reset();

                // Make an asynchronous Send request over the socket
                _socket.SendAsync(socketEventArg);

                // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
                // If no response comes back within this time then proceed
                _clientDone.WaitOne(TIMEOUT_MILLISECONDS);
            }
            
        }
    
    }
}
