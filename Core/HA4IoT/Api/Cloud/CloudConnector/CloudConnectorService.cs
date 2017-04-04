using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Api.Cloud;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Settings;
using Newtonsoft.Json;

namespace HA4IoT.Api.Cloud.CloudConnector
{
    public class CloudConnectorService : ServiceBase, IApiAdapter
    {
        private const string BaseUri = "https://ha4iot-cloudapi.azurewebsites.net/api/ControllerProxy";

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IApiDispatcherService _apiDispatcherService;
        private readonly CloudConnectorServiceSettings _settings;
        private readonly HttpClient _receivingHttpClient = new HttpClient();
        private readonly HttpClient _sendingHttpClient = new HttpClient();
        private readonly StringContent _emptyContent = new StringContent(string.Empty);
        private readonly Uri _receiveRequestsUri;
        private readonly Uri _sendResponseUri;
        private readonly ILogger _log;

        public CloudConnectorService(IApiDispatcherService apiDispatcherService, ISettingsService settingsService, ILogService logService)
        {
            if (apiDispatcherService == null) throw new ArgumentNullException(nameof(apiDispatcherService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (logService == null) throw new ArgumentNullException(nameof(logService));

            _log = logService.CreatePublisher(nameof(CloudConnectorService));

            _apiDispatcherService = apiDispatcherService;
            _receiveRequestsUri = new Uri($"{BaseUri}/ReceiveRequests");
            _sendResponseUri = new Uri($"{BaseUri}/SendResponse");

            _settings = settingsService.GetSettings<CloudConnectorServiceSettings>();

            _receivingHttpClient.DefaultRequestHeaders.Add(CloudConnectorHeaders.ControllerId, _settings.ControllerId);
            _receivingHttpClient.DefaultRequestHeaders.Add(CloudConnectorHeaders.ApiKey, _settings.ApiKey);
            _receivingHttpClient.Timeout = TimeSpan.FromMinutes(1.25);

            _sendingHttpClient.DefaultRequestHeaders.Add(CloudConnectorHeaders.ControllerId, _settings.ControllerId);
            _sendingHttpClient.DefaultRequestHeaders.Add(CloudConnectorHeaders.ApiKey, _settings.ApiKey);
        }

        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public void NotifyStateChanged(IComponent component)
        {
        }

        public override void Startup()
        {
            if (!_settings.IsEnabled)
            {
                _log.Info("Cloud Connector service is not enabled.");
                return;
            }

            _log.Info("Starting Cloud Connector service.");

            _apiDispatcherService.RegisterAdapter(this);

            Task.Factory.StartNew(
                async () => await ReceivePendingMessagesAsyncLoop(),
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        private async Task ReceivePendingMessagesAsyncLoop()
        {
            _log.Verbose("Started receiving pending Cloud messages.");
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var response = await ReceivePendingMessagesAsync();
                    if (response.Succeeded && !string.IsNullOrEmpty(response.Response))
                    {
                        Task.Run(() => ProcessPendingCloudMessages(response.Response)).Forget();
                    }
                }
                catch (Exception exception)
                {
                    _log.Error(exception, "Error while receiving pending Cloud messages.");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        private async Task<ReceivePendingMessagesAsyncResult> ReceivePendingMessagesAsync()
        {
            HttpResponseMessage result;
            try
            {
                result = await _receivingHttpClient.PostAsync(_receiveRequestsUri, _emptyContent);
            }
            catch (TaskCanceledException)
            {
                return new ReceivePendingMessagesAsyncResult();
            }
            
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content))
                {
                    return new ReceivePendingMessagesAsyncResult();
                }

                return new ReceivePendingMessagesAsyncResult
                {
                    Succeeded = true,
                    Response = content
                };
            }

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                _log.Warning("Credentials for Cloud access are invalid.");
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                _log.Warning("Cloud access is not working properly.");
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            else
            {
                _log.Warning($"Failed to receive pending Cloud message (Error code: {result.StatusCode}).");
            }
            
            return new ReceivePendingMessagesAsyncResult();
        }

        private async Task ProcessPendingCloudMessages(string content)
        {
            try
            {
                var pendingCloudMessages = JsonConvert.DeserializeObject<List<CloudRequestMessage>>(content);
                if (pendingCloudMessages == null)
                {
                    return;
                }

                foreach (var cloudMessage in pendingCloudMessages)
                {
                    var eventArgs = ProcessCloudMessage(cloudMessage);
                    await SendResponse(eventArgs);
                }

                _log.Verbose($"Handled {pendingCloudMessages.Count} pending Cloud messages.");
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Unhandled exception while processing cloud messages. " + exception.Message);
            }
            
        }

        private async Task SendResponse(CloudConnectorApiContext apiContext)
        {
            try
            {
                using (var content = CreateContent(apiContext))
                {
                    var result = await _sendingHttpClient.PostAsync(_sendResponseUri, content);
                    if (result.IsSuccessStatusCode)
                    {
                        _log.Verbose("Sent response message to Cloud.");
                    }
                    else
                    {
                        _log.Warning($"Failed to send response message to Cloud (Error code: {result.StatusCode}).");
                    }
                }
            }
            catch (Exception exception)
            {
                _log.Warning(exception, "Error while sending response message to cloud.");
            }
        }

        private CloudConnectorApiContext ProcessCloudMessage(CloudRequestMessage cloudMessage)
        {
            var apiContext = new CloudConnectorApiContext(cloudMessage);
            var eventArgs = new ApiRequestReceivedEventArgs(apiContext);

            RequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                apiContext.ResultCode = ApiResultCode.ActionNotSupported;
            }

            return apiContext;
        }

        private StringContent CreateContent(CloudConnectorApiContext apiContext)
        {
            var cloudMessage = new CloudResponseMessage();
            cloudMessage.Header.CorrelationId = apiContext.RequestMessage.Header.CorrelationId;
            cloudMessage.Response.ResultCode = apiContext.ResultCode;
            cloudMessage.Response.Result = apiContext.Result;
            
            var stringContent = new StringContent(JsonConvert.SerializeObject(cloudMessage));
            stringContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            stringContent.Headers.ContentEncoding.Add("utf-8");

            return stringContent;
        }
    }
}
