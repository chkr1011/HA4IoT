using System;
using Windows.Devices.I2c;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware
{
    public class I2CDeviceWrapper : II2CDevice
    {
        private readonly I2cDevice _i2CDevice;

        public I2CDeviceWrapper(I2cDevice i2cDevice)
        {
            if (i2cDevice == null) throw new ArgumentNullException(nameof(i2cDevice));

            _i2CDevice = i2cDevice;
        }

        public void Write(byte[] writeBuffer)
        {
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));

            _i2CDevice.Write(writeBuffer);
        }

        public void Read(byte[] readBuffer)
        {
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));

            _i2CDevice.Read(readBuffer);
        }

        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));
            
            _i2CDevice.WriteRead(writeBuffer, readBuffer);
        }
    }
}
