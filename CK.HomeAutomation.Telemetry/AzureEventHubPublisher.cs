using System;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using CK.HomeAutomation.Actuators;
using CK.HomeAutomation.Actuators.Contracts;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Telemetry
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

            SendToAzureEventHubAsync(data);
        }

        protected override void OnSensorValueChanged(BaseSensor sensor)
        {
            JsonObject data = CreateDataPackage(sensor.Id, EventType.SensorValueChanged);
            data.SetNamedValue("kind", JsonValue.CreateStringValue(sensor.GetType().Name));
            data.SetNamedValue("value", JsonValue.CreateNumberValue(sensor.Value));

            SendToAzureEventHubAsync(data);
        }

        protected override void OnBinaryStateActuatorStateChanged(IBinaryStateOutputActuator actuator, TimeSpan previousStateDuration)
        {
            JsonObject data = CreateDataPackage(actuator.Id, EventType.OutputActuatorStateChanged);
            data.SetNamedValue("state", JsonValue.CreateStringValue(actuator.State.ToString()));
            data.SetNamedValue("kind", JsonValue.CreateStringValue("Start"));
            SendToAzureEventHubAsync(data);

            BinaryActuatorState previousState = actuator.State == BinaryActuatorState.On ? BinaryActuatorState.Off : BinaryActuatorState.On;
            data = CreateDataPackage(actuator.Id, EventType.OutputActuatorStateChanged);
            data.SetNamedValue("state", JsonValue.CreateStringValue(previousState.ToString()));
            data.SetNamedValue("kind", JsonValue.CreateStringValue("End"));
            data.SetNamedValue("duration", JsonValue.CreateNumberValue(previousStateDuration.TotalSeconds));
            SendToAzureEventHubAsync(data);
        }

        protected override void OnStateMachineStateChanged(StateMachine stateMachine, StateMachineStateChangedEventArgs eventArgs, TimeSpan previousStateDuration)
        {
            JsonObject data = CreateDataPackage(stateMachine.Id, EventType.OutputActuatorStateChanged);
            data.SetNamedValue("state", JsonValue.CreateStringValue(eventArgs.NewState));
            data.SetNamedValue("kind", JsonValue.CreateStringValue("Start"));
            SendToAzureEventHubAsync(data);

            data = CreateDataPackage(stateMachine.Id, EventType.OutputActuatorStateChanged);
            data.SetNamedValue("state", JsonValue.CreateStringValue(eventArgs.OldState));
            data.SetNamedValue("kind", JsonValue.CreateStringValue("End"));
            data.SetNamedValue("duration", JsonValue.CreateNumberValue(previousStateDuration.TotalSeconds));
            SendToAzureEventHubAsync(data);
        }

        protected override void OnMotionDetected(IMotionDetector motionDetector)
        {
            JsonObject data = CreateDataPackage(motionDetector.Id, EventType.MotionDetected);
            data.SetNamedValue("kind", JsonValue.CreateStringValue("detected"));

            SendToAzureEventHubAsync(data);
        }

        private JsonObject CreateDataPackage(string actuatorId, EventType eventType)
        {
            JsonObject data = new JsonObject();
            data.SetNamedValue("timestamp", JsonValue.CreateStringValue(DateTime.Now.ToString("O")));
            data.SetNamedValue("type", JsonValue.CreateStringValue(eventType.ToString()));
            data.SetNamedValue("actuator", JsonValue.CreateStringValue(actuatorId));

            return data;
        }

        private async void SendToAzureEventHubAsync(JsonObject data)
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
