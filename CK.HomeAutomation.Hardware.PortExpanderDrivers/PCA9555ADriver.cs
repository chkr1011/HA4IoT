namespace CK.HomeAutomation.Hardware.PortExpanderDrivers
{
    public class PCA9555ADriver : PCA9555Driver
    {
        public PCA9555ADriver(int address, II2cBusAccessor i2CBus) : base(address, i2CBus)
        {
        }
    }
}