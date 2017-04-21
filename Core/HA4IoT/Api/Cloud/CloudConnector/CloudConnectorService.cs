using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Api.Cloud;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using Newtonsoft.Json;

namespace HA4IoT.Api.Cloud.CloudConnector
{
    public class CloudConnectorService : ServiceBase, IApiAdapter
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly StringContent _emptyContent = new StringContent(string.Empty);
        private readonly string _receiveRequestsUri;
        private readonly string _sendResponseUri;

        private readonly CloudConnectorServiceSettings _settings;
        private readonly IApiDispatcherService _apiDispatcherService;
        private readonly ILogger _log;

        private bool _isConnected;

        public CloudConnectorService(IApiDispatcherService apiDispatcherService, ISettingsService settingsService, ISystemInformationService systemInformationService, ILogService logService)
        {
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));
            _log = logService?.CreatePublisher(nameof(CloudConnectorService)) ?? throw new ArgumentNullException(nameof(logService));
            _apiDispatcherService = apiDispatcherService ?? throw new ArgumentNullException(nameof(apiDispatcherService));

            _settings = settingsService?.GetSettings<CloudConnectorServiceSettings>() ?? throw new ArgumentNullException(nameof(settingsService));

            _receiveRequestsUri = $"{_settings.ServerAddress}/api/ControllerProxy/ReceiveRequests";
            _sendResponseUri = $"{_settings.ServerAddress}/api/ControllerProxy/SendResponse";

            systemInformationService.Set("CloudConnector/IsConnected", () => _isConnected);
        }

        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public override void Startup()
        {
            if (!_settings.IsEnabled)
            {
                _log.Info("Cloud Connector service is disabled.");
                return;
            }

            _log.Info("Starting Cloud Connector service.");

            _apiDispatcherService.RegisterAdapter(this);

            Task.Run(ReceivePendingMessagesAsyncLoop, _cancellationTokenSource.Token);
        }

        public void NotifyStateChanged(IComponent component)
        {
        }

        private async Task ReceivePendingMessagesAsyncLoop()
        {
            _log.Verbose("Starting receiving pending Cloud messages.");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = CreateAuthorizationHeader();
                httpClient.Timeout = TimeSpan.FromMinutes(1.25);
                
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        var response = await ReceivePendingMessagesAsync(httpClient, _cancellationTokenSource.Token);
                        _isConnected = response.Succeeded;

                        if (response.Succeeded && !string.IsNullOrEmpty(response.Response))
                        {
                            Task.Run(() => ProcessPendingCloudMessages(response.Response)).Forget();
                        }
                    }
                    catch (Exception exception)
                    {
                        _log.Error(exception, "Error while receiving pending Cloud messages.");
                        _isConnected = false;

                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }
            }
        }

        private async Task<ReceivePendingMessagesAsyncResult> ReceivePendingMessagesAsync(HttpClient httpClient, CancellationToken cancellationToken)
        {
            HttpResponseMessage result;
            try
            {
                result = await httpClient.PostAsync(_receiveRequestsUri, _emptyContent, cancellationToken);
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
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
            else if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                _log.Warning("Cloud access is not working properly.");
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
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
                using (var httpClient = new HttpClient())
                using (var content = CreateContent(apiContext))
                {
                    httpClient.DefaultRequestHeaders.Authorization = CreateAuthorizationHeader();

                    var result = await httpClient.PostAsync(_sendResponseUri, content);
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

        private static StringContent CreateContent(CloudConnectorApiContext apiContext)
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

        private AuthenticationHeaderValue CreateAuthorizationHeader()
        {
            var value = $"{_settings.ControllerId}:{_settings.ApiKey}";
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
        }
    }
}
