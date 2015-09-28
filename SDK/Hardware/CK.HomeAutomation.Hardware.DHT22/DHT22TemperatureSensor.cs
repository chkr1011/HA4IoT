namespace CK.HomeAutomation.Hardware.DHT22
{
    public class DHT22TemperatureSensor : DHT22SensorBase
    {
        public DHT22TemperatureSensor(int sensorId, DHT22Accessor dht22Accessor) : base(sensorId, dht22Accessor)
        {
        }

        protected override float GetValueInternal(int sensorId, DHT22Accessor dht22Accessor)
        {
            return dht22Accessor.GetTemperature(sensorId);
        }
    }
}
