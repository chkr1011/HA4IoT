using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.Drivers.I2CPortExpanderDrivers
{
    public sealed class PCF8574Driver : II2CPortExpanderDriver
    {
        private readonly II2CBusService _i2CBus;
        private readonly I2CSlaveAddress _address;

        private readonly byte[] _readBuffer = new byte[1];
        private readonly byte[] _writeBuffer = new byte[1];

        public PCF8574Driver(I2CSlaveAddress address, II2CBusService i2CBus)
        {
            _address = address;
            _i2CBus = i2CBus ?? throw new ArgumentNullException(nameof(i2CBus));
        }

        public int StateSize => 1;

        public void Write(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Length != StateSize) throw new ArgumentException("Length is invalid.", nameof(state));

            _writeBuffer[0] = state[0];
            _i2CBus.Write(_address, _writeBuffer);
        }

        public byte[] Read()
        {
            _i2CBus.Read(_address, _readBuffer);
            return _readBuffer;
        }
    }
}