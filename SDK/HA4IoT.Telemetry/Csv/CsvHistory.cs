using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;
using HA4IoT.Telemetry.Statistics;

namespace HA4IoT.Telemetry.Csv
{
    public class CsvHistory : ActuatorMonitor
    {
        private readonly INotificationHandler _notificationHandler;
        private readonly IHttpRequestController _apiRequestController;
        private readonly string _filename;

        private readonly object _fileSyncRoot = new object();
        private readonly List<ActuatorHistoryEntry> _queuedEntries = new List<ActuatorHistoryEntry>();
        private readonly Dictionary<IActuator, ActuatorHistory> _actuatorHistory = new Dictionary<IActuator, ActuatorHistory>();

        public CsvHistory(INotificationHandler notificationHandler, IHttpRequestController apiRequestController)
        {
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));
            if (apiRequestController == null) throw new ArgumentNullException(nameof(apiRequestController));

            _notificationHandler = notificationHandler;
            _apiRequestController = apiRequestController;
            _filename = Path.Combine(ApplicationData.Current.LocalFolder.Path, "History.csv");

            Task.Factory.StartNew(WritePendingEntries, TaskCreationOptions.LongRunning);
        }

        public void ExposeToApi(IHttpRequestController httpRequestController)
        {
            if (httpRequestController == null) throw new ArgumentNullException(nameof(httpRequestController));

            httpRequestController.Handle(HttpMethod.Get, "history").Using(HandleApiGet);
        }

        protected override void OnActuatorConnecting(IActuator actuator)
        {
            _actuatorHistory[actuator] = new ActuatorHistory(actuator, _apiRequestController, _notificationHandler);
        }

        protected override void OnBinaryStateActuatorStateChanged(IBinaryStateOutputActuator actuator, BinaryActuatorState newState)
        {
            QueueEntry(actuator, newState.ToString());
        }

        protected override void OnSensorValueChanged(ISingleValueSensorActuator actuator, float newValue)
        {
            QueueEntry(actuator, newValue.ToString(CultureInfo.InvariantCulture));
        }

        protected override void OnStateMachineStateChanged(StateMachine stateMachine, string newState)
        {
            QueueEntry(stateMachine, newState);
        }

        private void QueueEntry(IActuator actuator, string newState)
        {
            var entry = new ActuatorHistoryEntry(DateTime.Now, actuator.Id, newState);
            _actuatorHistory[actuator].AddEntry(entry);

            lock (_queuedEntries)
            {
                _queuedEntries.Add(entry);
            }
        }

        private void WritePendingEntries()
        {
            while (true)
            {
                Task.Delay(100).Wait();

                var entries = new List<ActuatorHistoryEntry>();
                lock (_queuedEntries)
                {
                    entries.AddRange(_queuedEntries);
                    _queuedEntries.Clear();
                }

                if (entries.Count == 0)
                {
                    continue;
                }

                lock (_fileSyncRoot)
                {
                    foreach (var entry in entries)
                    {
                        try
                        {
                            File.AppendAllText(_filename, entry.ToCsv());
                        }
                        catch (Exception exception)
                        {
                            _notificationHandler.Warning("Error while write actuator state changes to CSV log. " + exception.Message);
                        }
                    }
                }
            }
        }

        private void HandleApiGet(HttpContext httpContext)
        {
            byte[] content;
            lock (_fileSyncRoot)
            {
                content = File.ReadAllBytes(_filename);
            }

            httpContext.Response.Body = new BinaryBody().WithContent(content).WithMimeType(MimeTypeProvider.Csv);
        }
    }
}
