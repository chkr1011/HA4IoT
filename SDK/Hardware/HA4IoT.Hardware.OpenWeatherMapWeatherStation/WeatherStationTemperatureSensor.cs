using System;
using HA4IoT.Actuators.Contracts;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class WeatherStationTemperatureSensor : ITemperatureSensor
    {
        public event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;

        public float Value { get; private set; }

        public float ValueChangedMinDelta { get; set; } = 0.15F;

        public void UpdateValue(float newValue)
        {
            float oldValue = Value;
            Value = newValue;

            if (Math.Abs(oldValue - newValue) > ValueChangedMinDelta)
            {
                ValueChanged?.Invoke(this, new SingleValueSensorValueChangedEventArgs(oldValue, newValue));
            }
        }
    }
}
