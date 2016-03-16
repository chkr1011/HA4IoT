using System;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using HA4IoT.Api.AzureCloud;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.CloudTester
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            
            Log.Instance = new TextBoxLogger(LogTextBox);

            var applicationData = Windows.Storage.ApplicationData.Current;
            var localSettings = applicationData.LocalSettings;

            if (localSettings == null)
            {
                return;
            }

            NamespaceTextBox.Text = Convert.ToString(localSettings.Values["NamespaceName"]);

            EventHubNameTextBox.Text = Convert.ToString(localSettings.Values["EventHubName"]);
            EventHubSasTokenTextBox.Text = Convert.ToString(localSettings.Values["EventHubSasToken"]);
            EventHubPublisherTextBox.Text = Convert.ToString(localSettings.Values["EventHubPublisher"]);

            InboundQueueNameTextBox.Text = Convert.ToString(localSettings.Values["InboundQueueName"]);
            InboundQueueSendSasTokenTextBox.Text = Convert.ToString(localSettings.Values["InboundQueueSasToken"]);
            InboundQueueListenSasTokenTextBox.Text = Convert.ToString(localSettings.Values["InboundQueueListenSasToken"]);

            OutboundQueueNameTextBox.Text = Convert.ToString(localSettings.Values["OutboundQueueName"]);
            OutboundQueueSendSasTokenTextBox.Text = Convert.ToString(localSettings.Values["OutboundQueueSasToken"]);
            OutboundQueueListenSasTokenTextBox.Text = Convert.ToString(localSettings.Values["OutboundQueueListenSasToken"]);
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            var applicationData = Windows.Storage.ApplicationData.Current;
            var localSettings = applicationData.LocalSettings;

            localSettings.Values["NamespaceName"] = NamespaceTextBox.Text;

            localSettings.Values["EventHubName"] = EventHubNameTextBox.Text;
            localSettings.Values["EventHubSasToken"] = EventHubSasTokenTextBox.Text;
            localSettings.Values["EventHubPublisher"] = EventHubPublisherTextBox.Text;

            localSettings.Values["InboundQueueName"] = InboundQueueNameTextBox.Text;
            localSettings.Values["InboundQueueSasToken"] = InboundQueueSendSasTokenTextBox.Text;
            localSettings.Values["InboundQueueListenSasToken"] = InboundQueueListenSasTokenTextBox.Text;

            localSettings.Values["OutboundQueueName"] = OutboundQueueNameTextBox.Text;
            localSettings.Values["OutboundQueueSasToken"] = OutboundQueueSendSasTokenTextBox.Text;
            localSettings.Values["OutboundQueueListenSasToken"] = OutboundQueueListenSasTokenTextBox.Text;

            applicationData.SignalDataChanged();
        }

        private void SendTestEvent(object sender, RoutedEventArgs e)
        {
            var eventHubSender = new EventHubSender(
                NamespaceTextBox.Text,
                EventHubNameTextBox.Text,
                EventHubPublisherTextBox.Text,
                EventHubSasTokenTextBox.Text);

            Task.Run(async () => await eventHubSender.SendAsync(new JsonObject()));
        }

        private void SendTestMessageToOutboundQueue(object sender, RoutedEventArgs e)
        {
            var queueSender = new QueueSender(
                NamespaceTextBox.Text, 
                OutboundQueueNameTextBox.Text,
                OutboundQueueSendSasTokenTextBox.Text);

           var properties = new JsonObject();
           var body = new JsonObject();

            Task.Run(async () => await queueSender.SendAsync(properties, body));
        }

        private void StartWaitForMessages(object sender, RoutedEventArgs e)
        {
            var queueReceiver = new QueueReceiver(
                NamespaceTextBox.Text,
                InboundQueueNameTextBox.Text,
                InboundQueueListenSasTokenTextBox.Text,
                TimeSpan.FromSeconds(60));

            queueReceiver.MessageReceived += LogMessage;
            queueReceiver.Start();
        }

        private void LogMessage(object sender, MessageReceivedEventArgs e)
        {
            new TextBoxLogger(LogTextBox).Info("MESSAGE: " + e.BrokerProperties.Stringify() + " " + e.Body.Stringify());
        }

        private void SendTestMessageToInboundQueue(object sender, RoutedEventArgs e)
        {
            var queueSender = new QueueSender(
                NamespaceTextBox.Text, 
                InboundQueueNameTextBox.Text,
                InboundQueueSendSasTokenTextBox.Text);

            var systemProperties = new JsonObject();
            systemProperties.SetNamedValue("CorrelationId", JsonValue.CreateStringValue(Guid.NewGuid().ToString()));
            
            var body = new JsonObject();
            body.SetNamedValue("CallType", JsonValue.CreateStringValue("Command"));
            body.SetNamedValue("Uri", JsonValue.CreateStringValue("/api/actuator/Office.LightCeilingFrontLeft/status"));

            var content = new JsonObject();
            content.SetNamedValue("state", JsonValue.CreateStringValue("Toggle"));
            //body.SetNamedValue("Message", JsonValue.CreateStringValue("Hello World"));

            body.SetNamedValue("Content", content);

            Task.Run(async () => await queueSender.SendAsync(systemProperties, body));
        }

        private void StartWaitForOutboundMessages(object sender, RoutedEventArgs e)
        {
            var queueReceiver = new QueueReceiver(
                NamespaceTextBox.Text,
                OutboundQueueNameTextBox.Text,
                OutboundQueueListenSasTokenTextBox.Text,
                TimeSpan.FromSeconds(60));

            queueReceiver.MessageReceived += LogMessage;
            queueReceiver.Start();
        }
    }
}
