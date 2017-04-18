using System;
using Windows.Devices.I2c;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.I2C
{
    public sealed class I2CDeviceWrapper : II2CDevice, IDisposable
    {
        private readonly I2cDevice _device;

        public I2CDeviceWrapper(I2cDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }

        public void Write(byte[] writeBuffer)
        {
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));

            _device.Write(writeBuffer);
        }

        public void Read(byte[] readBuffer)
        {
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));

            _device.Read(readBuffer);
        }

        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));

            _device.WriteRead(writeBuffer, readBuffer);
        }

        public void Dispose()
        {
            _device?.Dispose();
        }
    }
}
