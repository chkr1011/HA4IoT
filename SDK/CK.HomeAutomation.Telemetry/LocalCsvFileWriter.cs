using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using CK.HomeAutomation.Actuators.Contracts;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Telemetry
{
    public class LocalCsvFileWriter : ActuatorMonitor
    {
        private readonly string _filename;

        public LocalCsvFileWriter(INotificationHandler notificationHandler) : base(notificationHandler)
        {
            _filename = Path.Combine(ApplicationData.Current.LocalFolder.Path, "BinaryStateOutputActuatorChanges.csv");
        }

        protected override void OnBinaryStateActuatorStateChanged(IBinaryStateOutputActuator actuator, TimeSpan previousStateDuration)
        {
            Task.Run(() => WriteToLocalCsvFile(actuator));
        }

        private void WriteToLocalCsvFile(IBinaryStateOutputActuator actuator)
        {
            lock (_filename)
            {
                try
                {
                    // Template: {ISO_TIMESTAMP},{ACTUATOR_ID},{NEW_STATE}
                    string newLine = DateTime.Now.ToString("O") + "," + actuator.Id + "," + actuator.State;
                    File.AppendAllText(_filename, newLine + Environment.NewLine);
                }
                catch (Exception exception)
                {
                    NotificationHandler.PublishFrom(this, NotificationType.Warning, "Error while write actuator state changes to CSV log. {0}",
                        exception.Message);
                }
            }
        }
    }
}
