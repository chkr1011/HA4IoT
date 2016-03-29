using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Actuators
{
    public class HumiditySensor : SingleValueSensorBase, IHumiditySensor
    {
        public HumiditySensor(ActuatorId id, ISingleValueSensor sensor)
            : base(id)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            Settings.SetValue(SingleValueSensorSettings.MinDelta, 0.15F);
            sensor.ValueChanged += (s, e) => SetValueInternal(e.NewValue);
        }
    }
}