namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public class DHT22TemperatureSensor : DHT22SensorBase
    {
        public DHT22TemperatureSensor(int id, DHT22Accessor dht22Accessor) : base(id, dht22Accessor)
        {
        }

        protected override float GetValueInternal(int id, DHT22Accessor dht22Accessor)
        {
            return dht22Accessor.GetTemperature((byte)id);
        }
    }
}
