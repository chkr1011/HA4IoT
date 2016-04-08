using System;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Telemetry
{
    public class ActuatorHistoryEntry
    {
        public ActuatorHistoryEntry(DateTime timestamp, ComponentId actuatorId, IComponentState newState)
        {
            if (actuatorId == null) throw new ArgumentNullException(nameof(actuatorId));
            if (newState == null) throw new ArgumentNullException(nameof(newState));

            Timestamp = timestamp;
            ActuatorId = actuatorId;

            NewState = newState;
        }

        public DateTime Timestamp { get; }

        public ComponentId ActuatorId { get; }

        public IComponentState NewState { get; }

        public string ToCsv()
        {
            // Template: {ISO_TIMESTAMP_UTC},{ACTUATOR_ID},{NEW_STATE}
            return Timestamp.ToString("O") + "," + ActuatorId + "," + NewState + Environment.NewLine;
        }
    }
}
