using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.I2C.I2CPortExpanderDrivers
{
    public class MAX7311Driver : I2CIPortExpanderDriver
    {
        private readonly II2CBusService _i2CBus;
        private readonly I2CSlaveAddress _address;

        private const byte InputPortRegisterA = 0;
        private const byte OutputPortRegisterA = 2;
        private const byte PolarityInversionRegisterA = 4;
        private const byte ConfigurationRegisterA = 6;

        public MAX7311Driver(I2CSlaveAddress address, II2CBusService i2CBus)
        {
            _address = address ?? throw new ArgumentNullException(nameof(address));
            _i2CBus = i2CBus ?? throw new ArgumentNullException(nameof(i2CBus));
        }

        public int StateSize { get; } = 2;

        public void Write(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Length != StateSize) throw new ArgumentException("Length is invalid.", nameof(state));
            
            // TODO: Create one long buffer for all registers.
            _i2CBus.Write(_address, new byte[] { ConfigurationRegisterA, 0x0, 0x0 });
            _i2CBus.Write(_address, new[] { OutputPortRegisterA, state[0], state[1] });
        }

        public byte[] Read()
        {
            var buffer = new byte[StateSize];
            _i2CBus.WriteRead(_address, new[] { InputPortRegisterA }, buffer);

            return buffer;
        }
    }
}