using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.Sensors.TemperatureSensors
{
    public class TemperatureSensor : SensorBase, ITemperatureSensor
    {
        private readonly ISettingsService _settingsService;

        public TemperatureSensor(ComponentId id, ISettingsService settingsService, INumericValueSensorEndpoint endpoint)
            : base(id)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            _settingsService = settingsService;

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

        public override IList<IComponentState> GetSupportedStates()
        {
            return new List<IComponentState>();
        }

        private bool GetDifferenceIsLargeEnough(float value)
        {
            var settings = _settingsService.GetSettings<SingleValueSensorSettings>(Id);

            return Math.Abs(GetCurrentNumericValue() - value) >= settings.MinDelta;
        }
    }
}