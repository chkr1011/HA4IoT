using System;

namespace CK.HomeAutomation.Hardware.Drivers
{
    public class PCF8574Driver : IDeviceDriver
    {
        private readonly I2CBus _i2CBus;
        private readonly int _i2CDeviceAddress;

        public PCF8574Driver(int i2cDeviceAddress, I2CBus i2cBus)
        {
            if (i2cBus == null) throw new ArgumentNullException(nameof(i2cBus));

            _i2CDeviceAddress = i2cDeviceAddress;
            _i2CBus = i2cBus;
        }

        public int StateSize { get; } = 1;

        public void Write(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Length != StateSize) throw new ArgumentException("Length is invalid.", nameof(state));

            // TODO: Check error handling here if device is currently not available.
            _i2CBus.Execute(_i2CDeviceAddress, bus => bus.Write(state));
        }

        public byte[] Read()
        {
            // TODO: Check error handling here if device is currently not available.
            var buffer = new byte[StateSize];
            _i2CBus.Execute(_i2CDeviceAddress, bus => bus.Read(buffer));

            return buffer;
        }
    }
}