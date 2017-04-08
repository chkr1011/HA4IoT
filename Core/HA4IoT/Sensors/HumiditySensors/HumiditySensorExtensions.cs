using System;
using HA4IoT.Components;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Sensors.Triggers;

namespace HA4IoT.Sensors.HumiditySensors
{
    public static class HumiditySensorExtensions
    {
        public static ITrigger GetHumidityReachedTrigger(this IComponent component, float value, float delta = 5)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return new SensorValueThresholdTrigger(component, s =>
            {
                float? v;
                s.TryGetHumidity(out v);
                return v;
            },
            SensorValueThresholdMode.Reached).WithTarget(value).WithDelta(delta);
        }

        public static ITrigger GetHumidityUnderranTrigger(this IComponent component, float value, float delta = 5)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return new SensorValueThresholdTrigger(component, s =>
            {
                float? v;
                s.TryGetHumidity(out v);
                return v;
            },
            SensorValueThresholdMode.Underran).WithTarget(value).WithDelta(delta);
        }

        public static IHumiditySensor GetHumiditySensor(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IHumiditySensor>($"{area.Id}.{id}");
        }
    }
}
