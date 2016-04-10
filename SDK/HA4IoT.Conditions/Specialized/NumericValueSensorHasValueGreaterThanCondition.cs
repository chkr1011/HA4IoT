using System;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Conditions.Specialized
{
    public class NumericValueSensorHasValueGreaterThanCondition : Condition
    {
        public NumericValueSensorHasValueGreaterThanCondition(INumericValueSensor sensor, float value)
        {
            if (sensor == null) throw new ArgumentNullException(nameof(sensor));

            WithExpression(() => sensor.GetCurrentNumericValue() > value);
        }
    }
}
