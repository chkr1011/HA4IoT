using System;
using Microsoft.ServiceBus;

namespace HA4IoT.SasTokenGenerator
{
    public class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("https://ha4iot.servicebus.windows.net");

            Console.Write("Enter key: ");
            string key = Console.ReadLine();

            string token = SharedAccessSignatureTokenProvider.GetPublisherSharedAccessSignature(
                uri,
                "events",
                "Main", 
                "Send",
                key,
                TimeSpan.FromDays(365));

            Console.WriteLine("Token:");
            Console.WriteLine(token);

            Console.ReadLine();
        }
    }
}
