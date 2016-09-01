using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.Sensors.HumiditySensors
{
    public class HumiditySensor : SensorBase, IHumiditySensor
    {
        public HumiditySensor(ComponentId id, ISettingsService settingsService, INumericValueSensorEndpoint endpoint)
            : base(id)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            settingsService.CreateSettingsMonitor<SingleValueSensorSettings>(Id, s => Settings = s);

            SetState(new NumericSensorValue(0));

            endpoint.ValueChanged += (s, e) =>
            {
                if (!GetDifferenceIsLargeEnough(e.NewValue))
                {
                    return;
                }

                SetState(new NumericSensorValue(e.NewValue));
            };
        }

        public SingleValueSensorSettings Settings { get; private set; }

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
            return Math.Abs(GetCurrentNumericValue() - value) >= Settings.MinDelta;
        }
    }
}