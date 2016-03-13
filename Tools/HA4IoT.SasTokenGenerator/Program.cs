using System;
using Microsoft.ServiceBus;

namespace HA4IoT.SasTokenGenerator
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.Write("Enter key: ");
            string key = Console.ReadLine();
            Console.Write("Enter shared access policy: ");
            string policy = Console.ReadLine();

            Console.WriteLine("EventHub token:");
            Console.WriteLine(GenerateKeyForEventHub("ha4iot", "events", "main", policy, key, TimeSpan.FromDays(365)));

            Console.WriteLine("ServiceBus queue token:");
            Console.WriteLine(GenerateKeyForServiceBus("ha4iot", "ha4iot.controller.main-inbound", policy, key, TimeSpan.FromDays(365)));

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
        }

        private static string GenerateKeyForServiceBus(
            string namespaceName,
            string queueName,
            string sharedAccessPolicyName,
            string sharedAccessPolicyKey,
            TimeSpan timeToLive)
        {
            var uri = $"https://{namespaceName}.servicebus.windows.net/{queueName}/messages";

            return SharedAccessSignatureTokenProvider.GetSharedAccessSignature(
                sharedAccessPolicyName,
                sharedAccessPolicyKey,
                uri,
                timeToLive);
        }

        private static string GenerateKeyForEventHub(
            string namespaceName,
            string eventHubName,
            string publisherName,
            string sharedAccessPolicyName, 
            string sharedAccessPolicyKey,
            TimeSpan timeToLive)
        {
            var uri = new Uri($"https://{namespaceName}.servicebus.windows.net");

            return SharedAccessSignatureTokenProvider.GetPublisherSharedAccessSignature(
                uri,
                eventHubName,
                publisherName,
                sharedAccessPolicyName,
                sharedAccessPolicyKey,
                timeToLive);
        }
    }
}
