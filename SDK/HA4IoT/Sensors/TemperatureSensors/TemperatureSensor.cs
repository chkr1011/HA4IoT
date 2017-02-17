using System;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.Sensors.TemperatureSensors
{
    public class TemperatureSensor : SensorBase, ITemperatureSensor
    {
        public TemperatureSensor(string id, ISettingsService settingsService, ISensorAdapter endpoint)
            : base(id)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            settingsService.CreateSettingsMonitor<SingleValueSensorSettings>(Id, s => Settings = s);

            SetState(new GenericComponentState(0));

            endpoint.ValueChanged += (s, e) =>
            {
                // TODO: Create base class.
                if (!GetDifferenceIsLargeEnough(e.NewValue))
                {
                    return;
                }

                SetState(new GenericComponentState(e.NewValue));
            };
        }

        public SingleValueSensorSettings Settings { get; private set; }

        public float GetCurrentHumidity()
        {
            return GetState().Extract<TemperatureState>().Value ?? 0;
        }

        public override ComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection();
        }

        public override void InvokeCommand(ICommand command)
        {
            
        }

        private bool GetDifferenceIsLargeEnough(float value)
        {
            return Math.Abs(GetCurrentHumidity() - value) >= Settings.MinDelta;
        }
    }
}