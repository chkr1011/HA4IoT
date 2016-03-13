using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Api.AzureCloud
{
    public class QueueReceiver
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger _logger;
        private readonly Uri _uri;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private bool _wasStarted;

        public QueueReceiver(string namespaceName, string queueName, string sasToken, TimeSpan timeout, ILogger logger)
        {
            if (namespaceName == null) throw new ArgumentNullException(nameof(namespaceName));
            if (queueName == null) throw new ArgumentNullException(nameof(queueName));
            if (sasToken == null) throw new ArgumentNullException(nameof(sasToken));

            _logger = logger;
            _uri = new Uri($"https://{namespaceName}.servicebus.windows.net/{queueName}/messages/head?api-version=2015-01&timeout={(int)timeout.TotalSeconds}");

            _httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("SharedAccessSignature", sasToken);
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Start()
        {
            if (_wasStarted)
            {
                throw new InvalidOperationException("Starting multiple times is not allowed.");    
            }

            _wasStarted = true;

            Task.Factory.StartNew(
                async () => await WaitForMessages(),
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        private async Task WaitForMessages()
        {
            _logger.Verbose("Started waiting for messages on Azure queue.");
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    await WaitForMessage();
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, "Error while waiting for message.");
                }
            }
        }

        private async Task WaitForMessage()
        {
            // DELETE will force a "Receive & Delete".
            // POST will force a "Peek-Lock"
            HttpResponseMessage result = await _httpClient.DeleteAsync(_uri);
            if (result.StatusCode == HttpStatusCode.NoContent)
            {
                _logger.Verbose("Azure queue timeout reached. Reconnecting...");
                return;
            }

            if (result.IsSuccessStatusCode)
            {
                await HandleQueueMessage(result.Headers, result.Content);
            }
            else
            {
                _logger.Warning($"Failed to wait for Azure queue message (Error code: {result.StatusCode}).");
            }
        }

        private async Task HandleQueueMessage(HttpResponseHeaderCollection headers, IHttpContent content)
        {
            string brokerPropertiesSource;
            if (!headers.TryGetValue("BrokerProperties", out brokerPropertiesSource))
            {
                _logger.Warning("Received Azure queue message without broker properties.");
                return;
            }
            
            string bodySource = await content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(bodySource))
            {
                _logger.Warning("Received Azure queue message with empty body.");
                return;
            }

            JsonObject brokerProperties;
            if (!JsonObject.TryParse(brokerPropertiesSource, out brokerProperties))
            {
                _logger.Warning("Received Azure queue message with invalid broker properties.");
                return;
            }

            JsonObject body;
            if (!JsonObject.TryParse(bodySource, out body))
            {
                _logger.Warning("Received Azure queue message with not supported body (JSON expected).");
                return;
            }

            _logger.Verbose("Received valid Azure queue message.");
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(brokerProperties, body));
        }
    }
}
