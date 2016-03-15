using System;

namespace HA4IoT.Telemetry.History
{
    public class SensorDataPoint
    {
        public SensorDataPoint(DateTime timestamp, float value)
        {
            Timestamp = timestamp;
            Value = value;
        }

        public DateTime Timestamp { get; }

        public float Value { get; }
    }
}
