namespace CK.HomeAutomation.Hardware.DHT22
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
