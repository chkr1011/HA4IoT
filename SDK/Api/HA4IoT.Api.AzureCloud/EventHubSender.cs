using System;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Api.AzureCloud
{
    public class EventHubSender
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger _logger;
        private readonly Uri _uri;

        public EventHubSender(string namespaceName, string eventHubName, string sasToken, ILogger logger)
        {
            if (namespaceName == null) throw new ArgumentNullException(nameof(namespaceName));
            if (eventHubName == null) throw new ArgumentNullException(nameof(eventHubName));
            if (sasToken == null) throw new ArgumentNullException(nameof(sasToken));

            _logger = logger;
            _uri = new Uri($"https://{namespaceName}.servicebus.windows.net/{eventHubName}/messages");

            _httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("SharedAccessSignature", sasToken);
        }

        public void Send(JsonObject eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            Task.Run(() => SendToAzureEventHubAsync(eventData));
        }

        private async Task SendToAzureEventHubAsync(JsonObject data)
        {
            try
            {
                using (var content = CreateContent(data))
                {
                    HttpResponseMessage result = await _httpClient.PostAsync(_uri, content);
                    if (result.IsSuccessStatusCode)
                    {
                        _logger.Verbose("Azure event published successfully.");
                    }
                    else
                    {
                        _logger.Warning("Failed to publish azure event (Error code: {0}).", result.StatusCode);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Warning("Failed to publish azure event. {0}", exception.Message);
            }
        }

        private HttpStringContent CreateContent(JsonObject data)
        {
            var content = new HttpStringContent(data.Stringify());
            content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/atom+xml");
            content.Headers.ContentType.Parameters.Add(new HttpNameValueHeaderValue("type", "entry"));
            content.Headers.ContentType.CharSet = "utf-8";

            return content;
        }
    }
}
