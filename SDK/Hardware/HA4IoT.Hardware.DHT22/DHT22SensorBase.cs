using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.DHT22
{
    public abstract class DHT22SensorBase : ISingleValueSensor
    {
        private readonly int _id;
        private readonly DHT22Accessor _dht22Accessor;
        private float _value;

        protected DHT22SensorBase(int id, DHT22Accessor dht22Accessor)
        {
            if (dht22Accessor == null) throw new ArgumentNullException(nameof(dht22Accessor));

            _id = id;
            _dht22Accessor = dht22Accessor;

            dht22Accessor.ValuesUpdated += (s, e) => UpdateValue();
        }

        public event EventHandler<SingleValueSensorValueChangedEventArgs> ValueChanged;

        public float Read()
        {
            return _value;
        }

        private void UpdateValue()
        {
            float oldValue = _value;
            _value = GetValueInternal((byte)_id, _dht22Accessor);

            ValueChanged?.Invoke(this, new SingleValueSensorValueChangedEventArgs(oldValue, _value));
        }

        protected abstract float GetValueInternal(int sensorId, DHT22Accessor dht22Accessor);
    }
}
