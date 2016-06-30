using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.KnxFakeController
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting KNX Fake Controller...");

            TcpListener server = new TcpListener(IPAddress.Any, 8900);
            server.Start(10);

            Console.WriteLine("Waiting for clients...");
            while (true)
            {
                var client = server.AcceptSocket();
                Task.Factory.StartNew(() => HandleClient(client), TaskCreationOptions.LongRunning);
            }
        }

        private static void HandleClient(Socket client)
        {
            // TODO: Create dedicated class for clients!
            Console.WriteLine("Handling new client...");

            string request = ReadFromClient(client);

            if (request.StartsWith("p="))
            {
                SendToClient("p=ok", client);
            }
            
            request = ReadFromClient(client);

            SendToClient("d=ok", client);
        }

        private static void SendToClient(string response, Socket client)
        {
            string buffer = response + "\x03";
            client.Send(Encoding.UTF8.GetBytes(buffer));

            Console.WriteLine("Sent: " + buffer);
        }

        private static string ReadFromClient(Socket client)
        {
            var inputBuffer = new byte[64];
            int inputBufferLength = client.Receive(inputBuffer, 0, inputBuffer.Length, SocketFlags.Partial);
            Array.Resize(ref inputBuffer, inputBufferLength);

            var request = Encoding.UTF8.GetString(inputBuffer);

            Console.WriteLine("Received: " + request);

            return request;
        }
    }
}
