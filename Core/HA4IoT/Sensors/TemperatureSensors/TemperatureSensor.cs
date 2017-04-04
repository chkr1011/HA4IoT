using System;
using HA4IoT.Components;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.Sensors.TemperatureSensors
{
    public class TemperatureSensor : ComponentBase, ITemperatureSensor
    {
        private float? _value;

        public TemperatureSensor(string id, INumericSensorAdapter adapter, ISettingsService settingsService)
            : base(id)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            
            settingsService.CreateComponentSettingsMonitor<SingleValueSensorSettings>(Id, s => Settings = s);

            adapter.ValueChanged += (s, e) => Update(e.Value);
        }

        public SingleValueSensorSettings Settings { get; private set; }

        public override IComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection()
                .With(new TemperatureMeasurementFeature());
        }

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(new TemperatureState(_value));
        }

        private void Update(float? newValue)
        {
            if (!GetDifferenceIsLargeEnough(newValue))
            {
                return;
            }

            var oldState = GetState();
            _value = newValue;
            OnStateChanged(oldState);
        }

        private bool GetDifferenceIsLargeEnough(float? newValue)
        {
            if (_value.HasValue != newValue.HasValue)
            {
                return true;
            }

            if (!_value.HasValue)
            {
                return false;
            }

            return Math.Abs(_value.Value - newValue.Value) >= Settings.MinDelta;
        }
    }
}