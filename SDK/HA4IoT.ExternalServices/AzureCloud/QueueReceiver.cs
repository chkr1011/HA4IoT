using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using HA4IoT.Contracts.Logging;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ExternalServices.AzureCloud
{
    public class QueueReceiver
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly Uri _uri;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private bool _isEnabled;

        public QueueReceiver(string namespaceName, string queueName, string authorization, TimeSpan timeout)
        {
            if (namespaceName == null) throw new ArgumentNullException(nameof(namespaceName));
            if (queueName == null) throw new ArgumentNullException(nameof(queueName));
            if (authorization == null) throw new ArgumentNullException(nameof(authorization));

            _uri = new Uri($"https://{namespaceName}.servicebus.windows.net/{queueName}/messages/head?api-version=2015-01&timeout={(int)timeout.TotalSeconds}");

            _httpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", authorization);
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Enable()
        {
            if (_isEnabled)
            {
                throw new InvalidOperationException("Starting multiple times is not allowed.");    
            }

            _isEnabled = true;

            var task = Task.Factory.StartNew(
                WaitForMessages,
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            task.ConfigureAwait(false);
        }

        public void Diable()
        {
            _cancellationTokenSource.Cancel();
            _isEnabled = false;
        }

        private void WaitForMessages()
        {
            Log.Verbose("Started waiting for messages on Azure queue.");
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    WaitForMessage();
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Error while waiting for message.");
                }
            }
        }

        private void WaitForMessage()
        {
            // DELETE will force a "Receive & Delete".
            // POST will force a "Peek-Lock"
            var result = _httpClient.DeleteAsync(_uri).AsTask().Result;
            if (result.StatusCode == HttpStatusCode.NoContent)
            {
                Log.Verbose("Azure queue timeout reached. Reconnecting...");
                return;
            }

            if (result.IsSuccessStatusCode)
            {
                var task = Task.Factory.StartNew(
                    async () => await HandleQueueMessage(result.Headers, result.Content),
                    _cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning, 
                    TaskScheduler.Default);

                task.ConfigureAwait(false);
            }
            else
            {
                Log.Warning($"Failed to wait for Azure queue message (Error code: {result.StatusCode}).");
            }
        }

        private async Task HandleQueueMessage(HttpResponseHeaderCollection headers, IHttpContent content)
        {
            string brokerPropertiesSource;
            if (!headers.TryGetValue("BrokerProperties", out brokerPropertiesSource))
            {
                Log.Warning("Received Azure queue message without broker properties.");
                return;
            }
            
            var bodySource = await content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(bodySource))
            {
                Log.Warning("Received Azure queue message with empty body.");
                return;
            }

            var brokerProperties = JObject.Parse(brokerPropertiesSource);
            var body = JObject.Parse(bodySource);

            Log.Verbose("Received valid Azure queue message.");
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(brokerProperties, body));
        }
    }
}
