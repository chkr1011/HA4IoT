using Windows.Devices.I2c;

namespace CK.HomeAutomation.Hardware.I2CHardwareBridge
{
    public abstract class I2CHardwareBridgeCommand
    {
        public abstract void Execute(I2cDevice i2CDevice);
    }
}
