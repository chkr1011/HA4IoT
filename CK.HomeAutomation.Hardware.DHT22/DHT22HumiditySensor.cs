namespace CK.HomeAutomation.Hardware.DHT22
{
    public class DHT22HumiditySensor : DHT22SensorBase
    {
        public DHT22HumiditySensor(int sensorId, DHT22Accessor dht22Accessor) : base(sensorId, dht22Accessor)
        {
        }

        protected override float GetValueInternal(int sensorId, DHT22Accessor dht22Accessor)
        {
            return dht22Accessor.GetHumidity(sensorId);
        }
    }
}
