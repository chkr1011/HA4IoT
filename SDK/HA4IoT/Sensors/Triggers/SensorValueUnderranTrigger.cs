using System;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors.Triggers
{
    public class SensorValueUnderranTrigger : Trigger
    {
        private bool _invoked;

        public SensorValueUnderranTrigger(INumericValueSensor sensor)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            sensor.StateChanged += (s, e) => CheckValue(sensor);
        }

        public float Target { get; set; }

        public float Delta { get; set; }

        public SensorValueUnderranTrigger WithTarget(float target)
        {
            Target = target;
            return this;
        }

        public SensorValueUnderranTrigger WithDelta(float delta)
        {
            Delta = delta;
            return this;
        }

        private void CheckValue(INumericValueSensor sensor)
        {
            var newValue = sensor.GetCurrentNumericValue();

            if (newValue < Target)
            {
                if (_invoked)
                {
                    return;
                }

                _invoked = true;
                Execute();

                return;
            }

            if (newValue > Target + Delta)
            {
                _invoked = false;
            }
        }
    }
}
