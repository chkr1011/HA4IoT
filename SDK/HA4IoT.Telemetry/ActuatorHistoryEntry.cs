using System;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Telemetry
{
    public class ActuatorHistoryEntry
    {
        public ActuatorHistoryEntry(DateTime timestamp, ComponentId actuatorId, string newState)
        {
            Timestamp = timestamp;
            ActuatorId = actuatorId;

            NewState = newState;
        }

        public DateTime Timestamp { get; }

        public ComponentId ActuatorId { get; }

        public string NewState { get; }

        public string ToCsv()
        {
            // Template: {ISO_TIMESTAMP_UTC},{ACTUATOR_ID},{NEW_STATE}
            return Timestamp.ToString("O") + "," + ActuatorId + "," + NewState + Environment.NewLine;
        }
    }
}
