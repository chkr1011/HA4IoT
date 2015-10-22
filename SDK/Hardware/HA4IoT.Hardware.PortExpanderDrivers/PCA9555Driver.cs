namespace HA4IoT.Hardware.PortExpanderDrivers
{
    public class PCA9555Driver : MAX7311Driver
    {
        public PCA9555Driver(int address, II2cBusAccessor i2CBus) : base(address, i2CBus)
        {
        }
    }
}