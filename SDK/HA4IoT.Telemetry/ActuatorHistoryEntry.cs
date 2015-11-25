using System;

namespace HA4IoT.Telemetry
{
    public class ActuatorHistoryEntry
    {
        public ActuatorHistoryEntry(DateTime timestamp, string actuatorId, string newState)
        {
            Timestamp = timestamp;
            ActuatorId = actuatorId;

            NewState = newState;
        }

        public DateTime Timestamp { get; }

        public string ActuatorId { get; }

        public string NewState { get; }

        public string ToCsv()
        {
            // Template: {ISO_TIMESTAMP_UTC},{ACTUATOR_ID},{NEW_STATE}
            return Timestamp.ToString("O") + "," + ActuatorId + "," + NewState + Environment.NewLine;
        }
    }
}
