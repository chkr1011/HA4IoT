using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.Drivers.I2CPortExpanderDrivers
{
    public sealed class MAX7311Driver : II2CPortExpanderDriver
    {
        private readonly II2CBusService _i2CBus;
        private readonly I2CSlaveAddress _address;

        // Byte 0 = Offset
        // Register 0-1=Input
        // Register 2-3=Output
        // Register 4-5=Inversion
        // Register 6-7=Configuration
        // Register 8=Timeout
        private readonly byte[] _inputWriteBuffer = { 0 };
        private readonly byte[] _inputReadBuffer = new byte[2];
        private readonly byte[] _outputWriteBuffer = { 2, 0, 0 };
        private readonly byte[] _configurationWriteBuffer = { 6, 0, 0 };
        
        public MAX7311Driver(I2CSlaveAddress address, II2CBusService i2CBus)
        {
            _address = address;
            _i2CBus = i2CBus ?? throw new ArgumentNullException(nameof(i2CBus));
        }

        public int StateSize => 2;

        public void Write(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Length != StateSize) throw new ArgumentException("Length is invalid.", nameof(state));

            // Set configuration to output.
            _i2CBus.Write(_address, _configurationWriteBuffer);

            // Update the output registers only.
            _outputWriteBuffer[1] = state[0];
            _outputWriteBuffer[2] = state[1];

            _i2CBus.Write(_address, _outputWriteBuffer);
        }

        public byte[] Read()
        {
            _i2CBus.WriteRead(_address, _inputWriteBuffer, _inputReadBuffer);
            return _inputReadBuffer;
        }
    }
}