using System;
using HA4IoT.Actuators;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class WeatherStationTemperatureSensor : SingleValueSensorBase, ITemperatureSensor
    {
        public WeatherStationTemperatureSensor(ActuatorId id) 
            : base(id)
        {
        }

        public void SetValue(double value)
        {
            SetValueInternal(Convert.ToSingle(value));
        }
    }
}
