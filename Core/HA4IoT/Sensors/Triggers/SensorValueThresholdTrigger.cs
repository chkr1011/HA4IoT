using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Sensors.Triggers
{
    public class SensorValueThresholdTrigger : Trigger
    {
        private readonly SensorValueThresholdMode _mode;
        private readonly Func<IComponent, float?> _valueResolver;
        private bool _invoked;

        public SensorValueThresholdTrigger(IComponent component, Func<IComponent, float?> valueResolver, SensorValueThresholdMode mode)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            component.StateChanged += (s, e) => CheckValue(component);

            _mode = mode;
            _valueResolver = valueResolver ?? throw new ArgumentNullException(nameof(valueResolver));
        }

        public float Target { get; set; }

        public float Delta { get; set; }

        public SensorValueThresholdTrigger WithTarget(float target)
        {
            Target = target;
            return this;
        }

        public SensorValueThresholdTrigger WithDelta(float delta)
        {
            Delta = delta;
            return this;
        }

        private void CheckValue(IComponent sensor)
        {
            var newValue = _valueResolver(sensor);

            if (_mode == SensorValueThresholdMode.Reached)
            {
                if (newValue >= Target)
                {
                    if (_invoked)
                    {
                        return;
                    }

                    _invoked = true;
                    Execute();

                    return;
                }

                if (newValue < Target - Delta)
                {
                    _invoked = false;
                }
            }
            else if (_mode == SensorValueThresholdMode.Underran)
            {
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
}
