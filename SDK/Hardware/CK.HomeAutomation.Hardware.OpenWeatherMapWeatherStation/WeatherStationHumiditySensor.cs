using System;
using CK.HomeAutomation.Actuators.Contracts;

namespace CK.HomeAutomation.Hardware.OpenWeatherMapWeatherStation
{
    public class WeatherStationHumiditySensor : IHumiditySensor
    {
        public event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;

        public float Value { get; private set; }

        public void UpdateValue(float newValue)
        {
            float oldValue = Value;
            Value = newValue;

            ValueChanged?.Invoke(this, new SingleValueSensorValueChangedEventArgs(oldValue, newValue));
        }
    }
}
