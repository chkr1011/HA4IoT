using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.I2C.I2CPortExpanderDrivers
{
    public class PCF8574Driver : I2CIPortExpanderDriver
    {
        private readonly II2CBusService _i2CBus;
        private readonly I2CSlaveAddress _address;

        public PCF8574Driver(I2CSlaveAddress address, II2CBusService i2CBus)
        {
            _address = address ?? throw new ArgumentNullException(nameof(address));
            _i2CBus = i2CBus ?? throw new ArgumentNullException(nameof(i2CBus));
        }

        public int StateSize { get; } = 1;

        public void Write(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Length != StateSize) throw new ArgumentException("Length is invalid.", nameof(state));

            _i2CBus.Write(_address, state);
        }

        public byte[] Read()
        {
            var buffer = new byte[StateSize];
            _i2CBus.Read(_address, buffer);

            return buffer;
        }
    }
}