using System;
using System.Threading.Tasks;
using Windows.Web.Http;
using HA4IoT.Contracts.Logging;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Api.Cloud.Azure
{
    public class QueueSender
    {
        private readonly QueueSenderOptions _options;
        private readonly ILogger _log;
        private readonly Uri _uri;
        
        public QueueSender(QueueSenderOptions options, ILogger log)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (log == null) throw new ArgumentNullException(nameof(log));

            _options = options;
            _log = log;

            _uri = new Uri($"https://{options.NamespaceName}.servicebus.windows.net/{options.QueueName}/messages");
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
                        _log.Verbose("Sent message to Azure queue.");
                    }
                    else
                    {
                        _log.Warning($"Failed to send Azure queue message (Error code: {result.StatusCode}).");
                    }
                }
            }
            catch (Exception exception)
            {
                _log.Warning(exception, "Error while sending Azure queue message.");
            }
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", _options.Authorization);
            
            return httpClient;
        }

        private HttpStringContent CreateContent(JObject body)
        {
            var content = new HttpStringContent(body.ToString());
            return content;
        }
    }
}
