using System;
using Windows.Data.Json;
using Windows.UI.Xaml;
using HA4IoT.Api.AzureCloud;

namespace HA4IoT.CloudTester
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

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
            OutboundQueueSasTokenTextBox.Text = Convert.ToString(localSettings.Values["OutboundQueueSasToken"]);
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
            localSettings.Values["OutboundQueueSasToken"] = OutboundQueueSasTokenTextBox.Text;

            applicationData.SignalDataChanged();
        }

        private void SendTestEvent(object sender, RoutedEventArgs e)
        {
            var eventHubSender = new EventHubSender(
                NamespaceTextBox.Text,
                EventHubNameTextBox.Text,
                EventHubPublisherTextBox.Text,
                EventHubSasTokenTextBox.Text,
                new TextBoxLogger(LogTextBox));

            eventHubSender.Send(new JsonObject());
        }

        private void SendTestMessageToOutboundQueue(object sender, RoutedEventArgs e)
        {
            var queueSender = new QueueSender(
                NamespaceTextBox.Text, 
                OutboundQueueNameTextBox.Text,
                OutboundQueueSasTokenTextBox.Text, new TextBoxLogger(LogTextBox));

           var properties = new JsonObject();
           var body = new JsonObject();

           queueSender.Send(properties, body);
        }

        private void StartWaitForMessages(object sender, RoutedEventArgs e)
        {
            var queueReceiver = new QueueReceiver(
                NamespaceTextBox.Text,
                InboundQueueNameTextBox.Text,
                InboundQueueListenSasTokenTextBox.Text,
                TimeSpan.FromSeconds(5),
                new TextBoxLogger(LogTextBox));

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
                InboundQueueSendSasTokenTextBox.Text, 
                new TextBoxLogger(LogTextBox));

            var systemProperties = new JsonObject();
            systemProperties.SetNamedValue("CorrelationId", JsonValue.CreateStringValue(Guid.NewGuid().ToString()));
            
            var body = new JsonObject();
            body.SetNamedValue("CallType", JsonValue.CreateStringValue("Command"));
            body.SetNamedValue("Uri", JsonValue.CreateStringValue("/api/actuator/Office.LightCeilingFrontLeft/status"));

            var content = new JsonObject();
            content.SetNamedValue("state", JsonValue.CreateStringValue("Toggle"));
            //body.SetNamedValue("Message", JsonValue.CreateStringValue("Hello World"));

            body.SetNamedValue("Content", content);

            queueSender.Send(systemProperties, body);
        }
    }
}
