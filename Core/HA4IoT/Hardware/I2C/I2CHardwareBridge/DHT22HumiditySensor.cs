namespace HA4IoT.Hardware.I2C.I2CHardwareBridge
{
    public class DHT22HumiditySensor : DHT22SensorBase
    {
        public DHT22HumiditySensor(int id, DHT22Accessor dht22Accessor) : base(id, dht22Accessor)
        {
        }

        protected override float GetValueInternal(int pin, DHT22Accessor dht22Accessor)
        {
            return dht22Accessor.GetHumidity((byte)pin);
        }
    }
}
