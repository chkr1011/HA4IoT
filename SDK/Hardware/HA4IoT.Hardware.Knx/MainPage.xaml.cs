using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HA4IoT.Hardware.Knx
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private WebServer webServer;

        public MainPage()
        {
            this.InitializeComponent();
            webServer = new WebServer();
        }

        private void button_CreateConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidateRemoteHost() && ValidateInput())
                {
                    // Attempt to connect to the echo server
                    Log(String.Format("Connecting to server '{0}' over port {1} (echo) ...", textBox_IpAdress.Text, textBox_IpPort.Text), true);
                    string result = webServer.Connect(textBox_IpAdress.Text, Convert.ToInt32(textBox_IpPort.Text));
                    Log(result, false);
                }
            }
            catch (Exception ex)
            {
                textBlock_Error.Text = @"Connection aborted: 
                    " + ex.Message;
            }
        }

        private void button_SendMessage_Click(object sender, RoutedEventArgs e)
        {
            Log(String.Format("Sending '{0}' to server ...", textBox_MessageToSend.Text), true);
            string result = webServer.SendMessage(textBox_MessageToSend.Text);
            Log(result, false);

            // Receive a response from the echo server
            Log("Requesting Receive ...", true);
            result = webServer.Receive();
            Log(result, false);
        }


        #region UI Validation
        /// <summary>
        /// Validates the txtInput TextBox
        /// </summary>
        /// <returns>True if the txtInput TextBox contains valid data, otherwise 
        /// False.
        ///</returns>
        private bool ValidateInput()
        {
            // txtInput must contain some text
            if (String.IsNullOrWhiteSpace(textBox_IpPort.Text))
            {
                txtOutput.Text += Environment.NewLine + "Please enter the Port to listen to";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the txtRemoteHost TextBox
        /// </summary>
        /// <returns>True if the txtRemoteHost contains valid data,
        /// otherwise False
        /// </returns>
        private bool ValidateRemoteHost()
        {
            // The txtRemoteHost must contain some text
            if (String.IsNullOrWhiteSpace(textBox_IpAdress.Text))
            {
                txtOutput.Text += Environment.NewLine + "Please enter a host name";
                return false;
            }

            return true;
        }
        #endregion

        #region Logging
        /// <summary>
        /// Log text to the txtOutput TextBox
        /// </summary>
        /// <param name="message">The message to write to the txtOutput TextBox</param>
        /// <param name="isOutgoing">True if the message is an outgoing (client to server)
        /// message, False otherwise.
        /// </param>
        /// <remarks>We differentiate between a message from the client and server 
        /// by prepending each line  with ">>" and "<<" respectively.</remarks>
        private void Log(string message, bool isOutgoing)
        {
            string direction = (isOutgoing) ? ">> " : "<< ";
            txtOutput.Text += Environment.NewLine + direction + message;
        }

        /// <summary>
        /// Clears the txtOutput TextBox
        /// </summary>
        private void ClearLog()
        {
            txtOutput.Text = String.Empty;
        }
        #endregion















        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.GetDeferral();
            await ThreadPool.RunAsync(workItem =>
            {
                webServer.ReceiveAsync();
            });
        }
    }
}
