namespace CK.HomeAutomation.Hardware.PortExpanderDrivers
{
    public class PCF8574ADriver : PCF8574Driver
    { 
        public PCF8574ADriver(int address, II2cBusAccessor i2CBus) : base(address, i2CBus)
        {
        }
    }
}