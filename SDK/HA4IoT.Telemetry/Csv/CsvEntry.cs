using System;

namespace HA4IoT.Telemetry.Csv
{
    public class CsvEntry
    {
        public CsvEntry(DateTime timestamp, string actuatorId, string newState)
        {
            Timestamp = timestamp;
            ActuatorId = actuatorId;
            NewState = newState;
        }

        public DateTime Timestamp { get; }

        public string ActuatorId { get; }

        public string NewState { get; }

        public override string ToString()
        {
            // Template: {ISO_TIMESTAMP},{ACTUATOR_ID},{NEW_STATE}
            return Timestamp.ToString("O") + "," + ActuatorId + "," + NewState + Environment.NewLine;
        }
    }
}
