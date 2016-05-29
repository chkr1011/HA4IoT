using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Api.AzureCloud
{
    public class EventHubSender
    {
        private readonly AutoResetEvent _eventsLock = new AutoResetEvent(false);
        private readonly List<JsonObject> _pendingEvents = new List<JsonObject>();

        private readonly Uri _uri;
        private readonly string _authorization;

        public EventHubSender(string namespaceName, string eventHubName, string publisherName, string authorization)
        {
            if (namespaceName == null) throw new ArgumentNullException(nameof(namespaceName));
            if (eventHubName == null) throw new ArgumentNullException(nameof(eventHubName));
            if (publisherName == null) throw new ArgumentNullException(nameof(publisherName));
            if (authorization == null) throw new ArgumentNullException(nameof(authorization));

            _uri = new Uri($"https://{namespaceName}.servicebus.windows.net/{eventHubName}/publishers/{publisherName}/messages");
            _authorization = authorization;
        }

        public void Enable()
        {
            Task.Factory.StartNew(
                async () => await ProcessPendingEventsAsync(),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void EnqueueEvent(JsonObject eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            lock (_pendingEvents)
            {
                _pendingEvents.Add(eventData);
            }

            _eventsLock.Set();
        }

        private async Task ProcessPendingEventsAsync()
        {
            while (true)
            {
                try
                {
                    List<JsonObject> pendingEvents;
                    lock (_pendingEvents)
                    {
                        pendingEvents = new List<JsonObject>(_pendingEvents);
                        _pendingEvents.Clear();
                    }

                    if (!pendingEvents.Any())
                    {
                        _eventsLock.WaitOne();
                        continue;
                    }

                    foreach (var pendingEvent in pendingEvents)
                    {
                        await SendToAzureEventHubAsync(pendingEvent);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Error while processing pending EventHub events.");
                }
            }
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
                        Log.Verbose("Sent event to Azure EventHub.");
                    }
                    else
                    {
                        Log.Warning($"Failed to send Azure EventHub event (Error code: {result.StatusCode}).");
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Error while sending Azure EventHub event.");
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
            content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/atom+xml");
            content.Headers.ContentType.Parameters.Add(new HttpNameValueHeaderValue("type", "entry"));
            content.Headers.ContentType.CharSet = "utf-8";

            return content;
        }
    }
}
