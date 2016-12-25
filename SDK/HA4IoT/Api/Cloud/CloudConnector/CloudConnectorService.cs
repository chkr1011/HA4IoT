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
using Newtonsoft.Json;

namespace HA4IoT.Api.Cloud.CloudConnector
{
    public class CloudConnectorService : ServiceBase, IApiAdapter
    {
        private const string _baseUri = "https://ha4iot-cloudapi.azurewebsites.net/api/ControllerProxy";

        private readonly IApiDispatcherService _apiDispatcherService;
        private readonly HttpClient _receivingHttpClient = new HttpClient();
        private readonly Uri _receiveRequestsUri;
        private readonly Uri _sendResponseUri;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        // TODO: To Settings
        private readonly Guid _controllerId = Guid.Parse("0f39add9-bc56-4d6d-b69b-9b8b1c1ac890");

        public CloudConnectorService(IApiDispatcherService apiDispatcherService)
        {
            if (apiDispatcherService == null) throw new ArgumentNullException(nameof(apiDispatcherService));

            _apiDispatcherService = apiDispatcherService;
            _receiveRequestsUri = new Uri($"{_baseUri}/ReceiveRequests");
            _sendResponseUri = new Uri($"{_baseUri}/SendResponse");

            _receivingHttpClient.DefaultRequestHeaders.TryAppendWithoutValidation(CloudConnectorHeaders.ControllerId, _controllerId.ToString());
            _receivingHttpClient.DefaultRequestHeaders.TryAppendWithoutValidation(CloudConnectorHeaders.ApiKey, "123456 TODO");
        }

        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;

        public void NotifyStateChanged(IComponent component)
        {
        }

        public override void Startup()
        {
            // TODO: Check settings here and skip startup if required.

            Log.Info($"Starting {nameof(CloudConnectorService)}.");

            var task = Task.Factory.StartNew(
                async () => await ReceivePendingMessages(),
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning, 
                TaskScheduler.Default);

            task.ConfigureAwait(false);

            _apiDispatcherService.RegisterAdapter(this);
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
                using (var httpClient = CreateHttpClient())
                using (var content = CreateContent(apiContext))
                {
                    var result = await httpClient.PostAsync(_sendResponseUri, content);
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

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAppendWithoutValidation(CloudConnectorHeaders.ControllerId, _controllerId.ToString());
            httpClient.DefaultRequestHeaders.TryAppendWithoutValidation(CloudConnectorHeaders.ApiKey, "1234 TODO");

            return httpClient;
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
