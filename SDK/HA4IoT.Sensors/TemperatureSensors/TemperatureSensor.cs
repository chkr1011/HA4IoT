using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors.TemperatureSensors
{
    public class TemperatureSensor : SensorBase, ITemperatureSensor
    {
        public TemperatureSensor(ComponentId id, INumericValueSensorEndpoint endpoint)
            : base(id)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            Settings.SetValue(SingleValueSensorSettings.MinDelta, 0.15F);

            SetState(new NumericSensorValue(0));

            endpoint.ValueChanged += (s, e) =>
            {
                // TODO: Create base class.
                if (!GetDifferenceIsLargeEnough(e.NewValue))
                {
                    return;
                }

                SetState(new NumericSensorValue(e.NewValue));
            };
        }

        public float GetCurrentNumericValue()
        {
            return ((NumericSensorValue) GetState()).Value;
        }

        private bool GetDifferenceIsLargeEnough(float value)
        {
            return Math.Abs(GetCurrentNumericValue() - value) >= Settings.GetFloat(SingleValueSensorSettings.MinDelta);
        }
    }
}