using System;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using HA4IoT.Actuators;
using HA4IoT.Actuators.Contracts;
using HA4IoT.Notifications;

namespace HA4IoT.Telemetry
{
    public class AzureEventHubPublisher : ActuatorMonitor
    {
        private readonly string _sasToken;
        private readonly Uri _uri;

        public AzureEventHubPublisher(string eventHubNamespace, string eventHubName, string sasToken, INotificationHandler notificationHandler) : base(notificationHandler)
        {
            if (eventHubNamespace == null) throw new ArgumentNullException(nameof(eventHubNamespace));
            if (eventHubName == null) throw new ArgumentNullException(nameof(eventHubName));
            if (sasToken == null) throw new ArgumentNullException(nameof(sasToken));

            _sasToken = sasToken;
            _uri = new Uri(string.Format("https://{0}.servicebus.windows.net/{1}/messages?timeout=60&api-version=2014-01", eventHubNamespace, eventHubName));
        }

        protected override void OnButtonPressed(IButton button, ButtonPressedDuration duration)
        {
            JsonObject data = CreateDataPackage(button.Id, EventType.ButtonPressed);
            data.SetNamedValue("kind", JsonValue.CreateStringValue(duration.ToString()));

            Task.Run(() => SendToAzureEventHubAsync(data));
        }

        protected override void OnSensorValueChanged(SingleValueSensorBase sensor)
        {
            JsonObject data = CreateDataPackage(sensor.Id, EventType.SensorValueChanged);
            data.SetNamedValue("kind", JsonValue.CreateStringValue(sensor.GetType().Name));
            data.SetNamedValue("value", JsonValue.CreateNumberValue(sensor.Value));

            Task.Run(() => SendToAzureEventHubAsync(data));
        }

        protected override void OnBinaryStateActuatorStateChanged(IBinaryStateOutputActuator actuator, TimeSpan previousStateDuration)
        {
            JsonObject startData = CreateDataPackage(actuator.Id, EventType.OutputActuatorStateChanged);
            startData.SetNamedValue("state", JsonValue.CreateStringValue(actuator.State.ToString()));
            startData.SetNamedValue("kind", JsonValue.CreateStringValue("Start"));
            Task.Run(() => SendToAzureEventHubAsync(startData));

            BinaryActuatorState previousState = actuator.State == BinaryActuatorState.On ? BinaryActuatorState.Off : BinaryActuatorState.On;
            JsonObject endData = CreateDataPackage(actuator.Id, EventType.OutputActuatorStateChanged);
            endData.SetNamedValue("state", JsonValue.CreateStringValue(previousState.ToString()));
            endData.SetNamedValue("kind", JsonValue.CreateStringValue("End"));
            endData.SetNamedValue("duration", JsonValue.CreateNumberValue(previousStateDuration.TotalSeconds));
            Task.Run(() => SendToAzureEventHubAsync(endData));
        }

        protected override void OnStateMachineStateChanged(StateMachine stateMachine, StateMachineStateChangedEventArgs eventArgs, TimeSpan previousStateDuration)
        {
            JsonObject startData = CreateDataPackage(stateMachine.Id, EventType.OutputActuatorStateChanged);
            startData.SetNamedValue("state", JsonValue.CreateStringValue(eventArgs.NewValue));
            startData.SetNamedValue("kind", JsonValue.CreateStringValue("Start"));
            Task.Run(() => SendToAzureEventHubAsync(startData));

            JsonObject endData = CreateDataPackage(stateMachine.Id, EventType.OutputActuatorStateChanged);
            endData.SetNamedValue("state", JsonValue.CreateStringValue(eventArgs.OldValue));
            endData.SetNamedValue("kind", JsonValue.CreateStringValue("End"));
            endData.SetNamedValue("duration", JsonValue.CreateNumberValue(previousStateDuration.TotalSeconds));
            Task.Run(() => SendToAzureEventHubAsync(endData));
        }

        protected override void OnMotionDetected(IMotionDetector motionDetector)
        {
            JsonObject data = CreateDataPackage(motionDetector.Id, EventType.MotionDetected);
            data.SetNamedValue("kind", JsonValue.CreateStringValue("detected"));

            Task.Run(() => SendToAzureEventHubAsync(data));
        }

        private JsonObject CreateDataPackage(string actuatorId, EventType eventType)
        {
            JsonObject data = new JsonObject();
            data.SetNamedValue("timestamp", JsonValue.CreateStringValue(DateTime.Now.ToString("O")));
            data.SetNamedValue("type", JsonValue.CreateStringValue(eventType.ToString()));
            data.SetNamedValue("actuator", JsonValue.CreateStringValue(actuatorId));

            return data;
        }

        private async Task SendToAzureEventHubAsync(JsonObject data)
        {
            try
            {
                using (var client = CreateHttpClient())
                using (var content = CreateContent(data))
                {
                    HttpResponseMessage result = await client.PostAsync(_uri, content);
                    if (result.IsSuccessStatusCode)
                    {
                        NotificationHandler.PublishFrom(this, NotificationType.Verbose, "Event published successfully.");
                    }
                    else
                    {
                        NotificationHandler.PublishFrom(this, NotificationType.Warning, "Failed to publish event (Error code: {0}).", result.StatusCode);
                    }
                }
            }
            catch (Exception exception)
            {
                NotificationHandler.PublishFrom(this, NotificationType.Warning, "Failed to publish event. {0}", exception.Message);
            }
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("SharedAccessSignature", _sasToken);

            return client;
        }

        private HttpStringContent CreateContent(JsonObject data)
        {
            var content = new HttpStringContent(data.Stringify());
            content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/atom+xml");
            content.Headers.ContentType.Parameters.Add(new HttpNameValueHeaderValue("type", "entry"));
            content.Headers.ContentType.CharSet = "utf-8";

            return content;
        }

        private enum EventType
        {
            OutputActuatorStateChanged,
            SensorValueChanged,
            ButtonPressed,
            MotionDetected,
        }
    }
}
