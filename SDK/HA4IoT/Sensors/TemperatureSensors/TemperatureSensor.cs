using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.Sensors.TemperatureSensors
{
    public class TemperatureSensor : SensorBase, ITemperatureSensor
    {
        public TemperatureSensor(ComponentId id, ISettingsService settingsService, INumericValueSensorEndpoint endpoint)
            : base(id)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            settingsService.CreateSettingsMonitor<SingleValueSensorSettings>(Id, s => Settings = s);

            SetState(new ComponentState(0));

            endpoint.ValueChanged += (s, e) =>
            {
                // TODO: Create base class.
                if (!GetDifferenceIsLargeEnough(e.NewValue))
                {
                    return;
                }

                SetState(new ComponentState(e.NewValue));
            };
        }

        public SingleValueSensorSettings Settings { get; private set; }

        public float GetCurrentNumericValue()
        {
            return GetState().ToObject<float>();
        }

        public override IList<ComponentState> GetSupportedStates()
        {
            return new List<ComponentState>();
        }

        private bool GetDifferenceIsLargeEnough(float value)
        {
            return Math.Abs(GetCurrentNumericValue() - value) >= Settings.MinDelta;
        }
    }
}