using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using HA4IoT.Contracts.Logging;
using HA4IoT.ExternalServices.AzureCloud;
using Newtonsoft.Json.Linq;

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

            eventHubSender.Enable();
            eventHubSender.EnqueueEvent(new JObject());
        }

        private void SendTestMessageToOutboundQueue(object sender, RoutedEventArgs e)
        {
            var options = new QueueSenderOptions
            {
                NamespaceName = NamespaceTextBox.Text,
                QueueName = OutboundQueueNameTextBox.Text,
                Authorization = OutboundQueueSendSasTokenTextBox.Text
            };

            var queueSender = new QueueSender(options);

            var properties = new JObject();
            var body = new JObject();

            Task.Run(async () => await queueSender.SendAsync(properties, body));
        }

        private void StartWaitForMessages(object sender, RoutedEventArgs e)
        {
            var options = new QueueReceiverOptions
            {
                NamespaceName = NamespaceTextBox.Text,
                QueueName = InboundQueueNameTextBox.Text,
                Authorization = InboundQueueListenSasTokenTextBox.Text
            };
            
            var queueReceiver = new QueueReceiver(options);
            queueReceiver.MessageReceived += LogMessage;
            queueReceiver.Enable();
        }

        private void LogMessage(object sender, MessageReceivedEventArgs e)
        {
            new TextBoxLogger(LogTextBox).Info("MESSAGE: " + e.BrokerProperties + " " + e.Body);
        }

        private void SendTestMessageToInboundQueue(object sender, RoutedEventArgs e)
        {
            var options = new QueueSenderOptions
            {
                NamespaceName = NamespaceTextBox.Text,
                QueueName = OutboundQueueNameTextBox.Text,
                Authorization = OutboundQueueSendSasTokenTextBox.Text
            };

            var queueSender = new QueueSender(options);

            var systemProperties = new JObject
            {
                ["CorrelationId"] = Guid.NewGuid().ToString()
            };

            var body = new JObject
            {
                ["CallType"] = "Command",
                ["Uri"] = "/api/component/Office.CombinedCeilingLights/status"
            };

            var content = new JObject
            {
                ["action"] = "nextState"
            };

            body["Content"] = content;

            Task.Run(async () => await queueSender.SendAsync(systemProperties, body));
        }

        private void StartWaitForOutboundMessages(object sender, RoutedEventArgs e)
        {
            var options = new QueueReceiverOptions
            {
                NamespaceName = NamespaceTextBox.Text,
                QueueName = InboundQueueNameTextBox.Text,
                Authorization = InboundQueueListenSasTokenTextBox.Text
            };

            var queueReceiver = new QueueReceiver(options);

            queueReceiver.MessageReceived += LogMessage;
            queueReceiver.Enable();
        }
    }
}
