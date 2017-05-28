using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.I2C.I2CPortExpanderDrivers
{
    public class MAX7311Driver : I2CIPortExpanderDriver
    {
        private readonly II2CBusService _i2CBus;
        private readonly I2CSlaveAddress _address;

        // Byte 0 = Offset
        // Register 0-1=Input
        // Register 2-3=Output
        // Register 4-5=Inversion
        // Register 6-7=Configuration
        // Register 8=Timeout
        private readonly byte[] _allRegistersWriteBuffer = { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

        private readonly byte[] _inputWriteBuffer = { 0 };
        private readonly byte[] _inputReadBuffer = new byte[2];

        public MAX7311Driver(I2CSlaveAddress address, II2CBusService i2CBus)
        {
            _address = address;
            _i2CBus = i2CBus ?? throw new ArgumentNullException(nameof(i2CBus));
        }

        public int StateSize { get; } = 2;

        public void Write(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Length != StateSize) throw new ArgumentException("Length is invalid.", nameof(state));

            // Update the output registers only.
            _allRegistersWriteBuffer[3] = state[0];
            _allRegistersWriteBuffer[4] = state[1];

            _i2CBus.Write(_address, _allRegistersWriteBuffer);
        }

        public byte[] Read()
        {
            _i2CBus.WriteRead(_address, _inputWriteBuffer, _inputReadBuffer);
            return _inputReadBuffer;
        }
    }
}