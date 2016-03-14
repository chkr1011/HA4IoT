using System;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Api.AzureCloud
{
    public class EventHubSender
    {
        private readonly ILogger _logger;
        private readonly Uri _uri;
        private readonly string _authorization;

        public EventHubSender(string namespaceName, string eventHubName, string publisherName, string authorization, ILogger logger)
        {
            if (namespaceName == null) throw new ArgumentNullException(nameof(namespaceName));
            if (eventHubName == null) throw new ArgumentNullException(nameof(eventHubName));
            if (publisherName == null) throw new ArgumentNullException(nameof(publisherName));
            if (authorization == null) throw new ArgumentNullException(nameof(authorization));

            _logger = logger;
            _uri = new Uri($"https://{namespaceName}.servicebus.windows.net/{eventHubName}/publishers/{publisherName}/messages");
            _authorization = authorization;
        }

        public async Task SendAsync(JsonObject eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            await SendToAzureEventHubAsync(eventData);
        }

        private async Task SendToAzureEventHubAsync(JsonObject body)
        {
            try
            {
                using (var httpClient = CreateHttpClient())
                using (var content = CreateContent(body))
                {
                    HttpResponseMessage result = await httpClient.PostAsync(_uri, content);
                    if (result.IsSuccessStatusCode)
                    {
                        _logger.Verbose("Sent event to Azure EventHub.");
                    }
                    else
                    {
                        _logger.Warning("Failed to send Azure EventHub event (Error code: {0}).", result.StatusCode);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Warning(exception, "Error while sending Azure EventHub event.");
            }
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", _authorization);

            return httpClient;
        }

        private HttpStringContent CreateContent(JsonObject data)
        {
            var content = new HttpStringContent(data.Stringify());
            ////content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/atom+xml");
            ////content.Headers.ContentType.Parameters.Add(new HttpNameValueHeaderValue("type", "entry"));
            ////content.Headers.ContentType.CharSet = "utf-8";

            return content;
        }
    }
}
