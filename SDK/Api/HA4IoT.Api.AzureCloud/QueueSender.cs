using System;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Api.AzureCloud
{
    public class QueueSender
    {
        private readonly ILogger _logger;
        private readonly Uri _uri;
        private readonly string _sasToken;

        public QueueSender(string namespaceName, string queueName, string sasToken, ILogger logger)
        {
            if (namespaceName == null) throw new ArgumentNullException(nameof(namespaceName));
            if (queueName == null) throw new ArgumentNullException(nameof(queueName));
            if (sasToken == null) throw new ArgumentNullException(nameof(sasToken));

            _logger = logger;
            _uri = new Uri($"https://{namespaceName}.servicebus.windows.net/{queueName}/messages");
            _sasToken = sasToken;
        }

        public async Task SendAsync(JsonObject brokerProperties, JsonObject body)
        {
            if (brokerProperties == null) throw new ArgumentNullException(nameof(brokerProperties));
            if (body == null) throw new ArgumentNullException(nameof(body));

            await SendToAzureQueueAsync(brokerProperties, body);
        }

        private async Task SendToAzureQueueAsync(JsonObject brokerProperties, JsonObject body)
        {
            try
            {
                using (var httpClient = CreateHttpClient())
                using (var content = CreateContent(body))
                {
                    httpClient.DefaultRequestHeaders.Add("BrokerProperties", brokerProperties.Stringify());

                    HttpResponseMessage result = await httpClient.PostAsync(_uri, content);
                    if (result.IsSuccessStatusCode)
                    {
                        _logger.Verbose("Sent message to Azure queue.");
                    }
                    else
                    {
                        _logger.Warning("Failed to send Azure queue message (Error code: {0}).", result.StatusCode);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Warning(exception, "Error while sending Azure queue message.");
            }
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("SharedAccessSignature", _sasToken);

            return httpClient;
        }

        private HttpStringContent CreateContent(JsonObject body)
        {
            var content = new HttpStringContent(body.Stringify());
            ////content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/atom+xml");
            ////content.Headers.ContentType.Parameters.Add(new HttpNameValueHeaderValue("type", "entry"));
            ////content.Headers.ContentType.CharSet = "utf-8";

            return content;
        }
    }
}
