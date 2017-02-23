using System;
using HA4IoT.Components;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Conditions.Specialized
{
    public class TemperatureIsGreaterThanCondition : Condition
    {
        public TemperatureIsGreaterThanCondition(IComponent component, float? threshold)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            WithExpression(() =>
            {
                float? value;
                component.TryGetTemperature(out value);

                return value > threshold;
            });
        }
    }
}
