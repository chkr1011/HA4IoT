namespace HA4IoT.Hardware.I2C.I2CHardwareBridge
{
    public class ReadDHT22SensorCommandResponse
    {
        public ReadDHT22SensorCommandResponse(bool succeeded, float temperature, float humidity)
        {
            Succeeded = succeeded;
            Temperature = temperature;
            Humidity = humidity;
        }

        public bool Succeeded { get; }

        public float Temperature { get; }

        public float Humidity { get; }
    }
}
