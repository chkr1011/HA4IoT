using System;
using System.Threading.Tasks;
using Windows.Web.Http;
using HA4IoT.Contracts.Logging;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ExternalServices.AzureCloud
{
    public class QueueSender
    {
        private readonly Uri _uri;
        private readonly string _authorization;

        public QueueSender(string namespaceName, string queueName, string authorization)
        {
            if (namespaceName == null) throw new ArgumentNullException(nameof(namespaceName));
            if (queueName == null) throw new ArgumentNullException(nameof(queueName));
            if (authorization == null) throw new ArgumentNullException(nameof(authorization));

            _uri = new Uri($"https://{namespaceName}.servicebus.windows.net/{queueName}/messages");
            _authorization = authorization;
        }

        public async Task SendAsync(JObject brokerProperties, JObject body)
        {
            if (brokerProperties == null) throw new ArgumentNullException(nameof(brokerProperties));
            if (body == null) throw new ArgumentNullException(nameof(body));

            await SendToAzureQueueAsync(brokerProperties, body);
        }

        private async Task SendToAzureQueueAsync(JObject brokerProperties, JObject body)
        {
            try
            {
                using (var httpClient = CreateHttpClient())
                using (var content = CreateContent(body))
                {
                    httpClient.DefaultRequestHeaders.Add("BrokerProperties", brokerProperties.ToString());

                    HttpResponseMessage result = await httpClient.PostAsync(_uri, content);
                    if (result.IsSuccessStatusCode)
                    {
                        Log.Verbose("Sent message to Azure queue.");
                    }
                    else
                    {
                        Log.Warning($"Failed to send Azure queue message (Error code: {result.StatusCode}).");
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Error while sending Azure queue message.");
            }
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", _authorization);
            
            return httpClient;
        }

        private HttpStringContent CreateContent(JObject body)
        {
            var content = new HttpStringContent(body.ToString());
            return content;
        }
    }
}
