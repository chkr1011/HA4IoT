using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
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
        private readonly Uri _receiveRequestsUri;
        private readonly Uri _sendResponseUri;
        
        public CloudConnectorService(IApiDispatcherService apiDispatcherService, ISettingsService settingsService)
        {
            if (apiDispatcherService == null) throw new ArgumentNullException(nameof(apiDispatcherService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _apiDispatcherService = apiDispatcherService;
            _receiveRequestsUri = new Uri($"{BaseUri}/ReceiveRequests");
            _sendResponseUri = new Uri($"{BaseUri}/SendResponse");

            _settings = settingsService.GetSettings<CloudConnectorServiceSettings>();

            _receivingHttpClient.DefaultRequestHeaders.TryAppendWithoutValidation(CloudConnectorHeaders.ControllerId, _settings.ControllerId);
            _receivingHttpClient.DefaultRequestHeaders.TryAppendWithoutValidation(CloudConnectorHeaders.ApiKey, _settings.ApiKey);

            _sendingHttpClient.DefaultRequestHeaders.TryAppendWithoutValidation(CloudConnectorHeaders.ControllerId, _settings.ControllerId);
            _sendingHttpClient.DefaultRequestHeaders.TryAppendWithoutValidation(CloudConnectorHeaders.ApiKey, _settings.ApiKey);
        }

        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public void NotifyStateChanged(IComponent component)
        {
        }

        public override void Startup()
        {
            if (!_settings.IsEnabled)
            {
                Log.Info("Cloud Connector service is not enabled.");
                return;
            }
            
            Log.Info($"Starting Cloud Connector service.");

            _apiDispatcherService.RegisterAdapter(this);

            var task = Task.Factory.StartNew(
                async () => await ReceivePendingMessages(),
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning, 
                TaskScheduler.Default);

            task.ConfigureAwait(false);           
        }

        private async Task ReceivePendingMessages()
        {
            Log.Verbose("Started receiving pending Cloud messages.");
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    await WaitForMessage();
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Error while receiving pending Cloud messages.");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        private async Task WaitForMessage()
        {
            var result = await _receivingHttpClient.PostAsync(_receiveRequestsUri, new HttpStringContent(string.Empty));

            if (result.IsSuccessStatusCode)
            {
                var task = Task.Factory.StartNew(
                    async () => await HandleCloudMessage(result.Content),
                    _cancellationTokenSource.Token);

                await task.ConfigureAwait(false);
            }
            else
            {
                if (result.StatusCode == HttpStatusCode.RequestTimeout || result.StatusCode == HttpStatusCode.NoContent)
                {
                    return;
                }

                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Log.Warning("Credentials for Cloud access are invalid.");
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }

                if (result.StatusCode == HttpStatusCode.InternalServerError)
                {
                    Log.Warning("Cloud access is not working properly.");
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }

                Log.Warning($"Failed to receive pending Cloud message (Error code: {result.StatusCode}).");
            }
        }

        private async Task HandleCloudMessage(IHttpContent content)
        {
            var pendingCloudMessages = JsonConvert.DeserializeObject<List<CloudRequestMessage>>(await content.ReadAsStringAsync());
            foreach (var cloudMessage in pendingCloudMessages)
            {
                var eventArgs = ProcessCloudMessage(cloudMessage);
                await SendResponse(eventArgs);
            }

            Log.Verbose($"Handled {pendingCloudMessages.Count} pending Cloud messages.");
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
                        Log.Verbose("Sent response message to Cloud.");
                    }
                    else
                    {
                        Log.Warning($"Failed to send response message to Cloud (Error code: {result.StatusCode}).");
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Error while sending response message to cloud.");
            }
        }

        private CloudConnectorApiContext ProcessCloudMessage(CloudRequestMessage cloudMessage)
        {
            var apiContext = new CloudConnectorApiContext(cloudMessage);
            var eventArgs = new ApiRequestReceivedEventArgs(apiContext);

            RequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                apiContext.ResultCode = ApiResultCode.NotSupported;
            }

            return apiContext;
        }

        private HttpStringContent CreateContent(CloudConnectorApiContext apiContext)
        {
            var cloudMessage = new CloudResponseMessage();
            cloudMessage.Header.CorrelationId = apiContext.RequestMessage.Header.CorrelationId;
            cloudMessage.Response.Action = apiContext.RequestMessage.Request.Action;
            cloudMessage.Response.Result = apiContext.Response;
            cloudMessage.Response.ResultCode = apiContext.ResultCode;
            cloudMessage.Response.InternalProcessingDuration = (int)(cloudMessage.Header.Created - apiContext.RequestMessage.Header.Created).TotalMilliseconds;
            
            var stringContent = new HttpStringContent(JsonConvert.SerializeObject(cloudMessage));
            stringContent.Headers.ContentType = HttpMediaTypeHeaderValue.Parse("application/json");

            return stringContent;
        }
    }
}
