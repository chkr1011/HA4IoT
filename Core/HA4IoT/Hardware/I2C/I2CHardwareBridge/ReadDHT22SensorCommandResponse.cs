namespace HA4IoT.Hardware.I2C.I2CHardwareBridge
{
    public class ReadDHT22SensorCommandResponse
    {
        public bool Succeeded { get; set; }

        public float Temperature { get; set; }

        public float Humidity { get; set; }
    }
}
