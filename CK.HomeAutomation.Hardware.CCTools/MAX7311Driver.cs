using System;

namespace CK.HomeAutomation.Hardware.CCTools
{
    public class MAX7311Driver : IDeviceDriver
    {
        private readonly I2CBus _i2CBus;
        private readonly int _i2CDeviceAddress;

        public MAX7311Driver(int i2cDeviceAddress, I2CBus i2cBus)
        {
            if (i2cBus == null) throw new ArgumentNullException(nameof(i2cBus));

            _i2CDeviceAddress = i2cDeviceAddress;
            _i2CBus = i2cBus;
        }

        public int StateSize { get; } = 2;

        public void Write(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Length != StateSize) throw new ArgumentException("Length is invalid.", nameof(state));

            // TODO: Check error handling here if device is currently not available.
            var setConfigurationToOutput = new byte[] {0x06, 0x00, 0x00};
            var setState = new byte[] { 0x02, state[0], state[1] };
            _i2CBus.Execute(_i2CDeviceAddress, bus =>
            {
                bus.Write(setConfigurationToOutput);
                bus.Write(setState);
            });
        }

        public byte[] Read()
        {
            // TODO: Check error handling here if device is currently not available.
            var inputRegister = new byte[] {0x00};
            var buffer = new byte[StateSize];
            _i2CBus.Execute(_i2CDeviceAddress, bus => bus.WriteRead(inputRegister, buffer));

            return buffer;
        }
    }
}