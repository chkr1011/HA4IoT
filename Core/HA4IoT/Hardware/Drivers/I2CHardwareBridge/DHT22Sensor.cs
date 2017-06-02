using System;
using HA4IoT.Contracts.Components.Adapters;

namespace HA4IoT.Hardware.Drivers.I2CHardwareBridge
{
    public class Dht22Sensor : INumericSensorAdapter
    {
        private readonly int _id;
        private readonly DHT22SensorKind _kind;
        private readonly DHT22Accessor _dht22Accessor;
        private float _value;

        public Dht22Sensor(int id, DHT22SensorKind kind, DHT22Accessor dht22Accessor)
        {
            _id = id;
            _kind = kind;
            _dht22Accessor = dht22Accessor ?? throw new ArgumentNullException(nameof(dht22Accessor));
            dht22Accessor.ValuesUpdated += UpdateValue;
        }

        public event EventHandler<NumericSensorAdapterValueChangedEventArgs> ValueChanged;

        public void Refresh()
        {
            UpdateValue(null, null);
        }

        private void UpdateValue(object sender, EventArgs eventArgs)
        {
            _value = GetValueInternal((byte)_id, _dht22Accessor);
            ValueChanged?.Invoke(this, new NumericSensorAdapterValueChangedEventArgs(_value));
        }

        public float GetValueInternal(int sensorId, DHT22Accessor dht22Accessor)
        {
            if (_kind == DHT22SensorKind.Temperature)
            {
                return dht22Accessor.GetTemperature((byte)sensorId);
            }

            if (_kind == DHT22SensorKind.Humidity)
            {
                return dht22Accessor.GetHumidity((byte)sensorId);
            }

            throw new NotSupportedException();
        }
    }
}
