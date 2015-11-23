using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Telemetry.Csv
{
    public class LocalCsvFileWriter : ActuatorMonitor
    {
        private readonly string _filename;

        private readonly object _fileSyncRoot = new object();
        private readonly List<CsvEntry> _queuedEntries = new List<CsvEntry>();

        public LocalCsvFileWriter(INotificationHandler notificationHandler) : base(notificationHandler)
        {
            _filename = Path.Combine(ApplicationData.Current.LocalFolder.Path, "BinaryStateOutputActuatorChanges.csv");

            Task.Factory.StartNew(WritePendingEntries, TaskCreationOptions.LongRunning);
        }

        public void ExposeToApi(IHttpRequestController httpRequestController)
        {
            if (httpRequestController == null) throw new ArgumentNullException(nameof(httpRequestController));

            httpRequestController.Handle(HttpMethod.Get, "telemetry").Using(HandleApiGet);
        }

        protected override void OnBinaryStateActuatorStateChanged(IBinaryStateOutputActuator actuator, BinaryActuatorStateChangedEventArgs eventArgs, TimeSpan previousStateDuration)
        {
            QueueEntry(actuator.Id, eventArgs.NewValue.ToString());
        }

        protected override void OnSensorValueChanged(SingleValueSensorBase sensor, SingleValueSensorValueChangedEventArgs eventArgs)
        {
            QueueEntry(sensor.Id, eventArgs.NewValue.ToString(CultureInfo.InvariantCulture));
        }

        protected override void OnStateMachineStateChanged(StateMachine stateMachine, StateMachineStateChangedEventArgs eventArgs,
            TimeSpan previousStateDuration)
        {
            QueueEntry(stateMachine.Id, eventArgs.NewValue);
        }

        private void QueueEntry(string actuatorId, string newState)
        {
            var entry = new CsvEntry(DateTime.Now, actuatorId, newState);

            lock (_queuedEntries)
            {
                _queuedEntries.Add(entry);
            }
        }

        private void WritePendingEntries()
        {
            while (true)
            {
                List<CsvEntry> entries = new List<CsvEntry>();
                lock (_queuedEntries)
                {
                    entries.AddRange(_queuedEntries);
                    _queuedEntries.Clear();
                }

                if (entries.Count == 0)
                {
                    Task.Delay(100).Wait();
                }

                lock (_fileSyncRoot)
                {
                    foreach (var entry in entries)
                    {
                        try
                        {
                            File.AppendAllText(_filename, entry.ToString());
                        }
                        catch (Exception exception)
                        {
                            NotificationHandler.Warning("Error while write actuator state changes to CSV log. " + exception.Message);
                        }
                    }
                }
            }
        }

        private void HandleApiGet(HttpContext httpContext)
        {
            string content;
            lock (_fileSyncRoot)
            {
                content = File.ReadAllText(_filename);
            }

            httpContext.Response.Body = new PlainTextBody().WithContent(content).WithMimeType(MimeTypeProvider.Csv);
        }
    }
}
