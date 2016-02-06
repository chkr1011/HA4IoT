using System;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Telemetry
{
    public class ActuatorHistoryEntry
    {
        public ActuatorHistoryEntry(DateTime timestamp, ActuatorId actuatorId, string newState)
        {
            Timestamp = timestamp;
            ActuatorId = actuatorId;

            NewState = newState;
        }

        public DateTime Timestamp { get; }

        public ActuatorId ActuatorId { get; }

        public string NewState { get; }

        public string ToCsv()
        {
            // Template: {ISO_TIMESTAMP_UTC},{ACTUATOR_ID},{NEW_STATE}
            return Timestamp.ToString("O") + "," + ActuatorId + "," + NewState + Environment.NewLine;
        }
    }
}
