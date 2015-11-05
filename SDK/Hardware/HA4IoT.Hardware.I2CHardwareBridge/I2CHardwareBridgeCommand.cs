using Windows.Devices.I2c;

namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public abstract class I2CHardwareBridgeCommand
    {
        public abstract void Execute(I2cDevice i2CDevice);
    }
}
