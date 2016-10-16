using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Hardware.PortExpanderDrivers
{
    public class MAX7311Driver : IPortExpanderDriver
    {
        private readonly II2CBusService _i2CBus;
        private readonly I2CSlaveAddress _address;

        private const byte InputPortRegisterA = 0;
        private const byte OutputPortRegisterA = 2;
        private const byte PolarityInversionRegisterA = 4;
        private const byte ConfigurationRegisterA = 6;
        
        public MAX7311Driver(I2CSlaveAddress address, II2CBusService i2CBus)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (i2CBus == null) throw new ArgumentNullException(nameof(i2CBus));

            _address = address;
            _i2CBus = i2CBus;
        }

        public int StateSize { get; } = 2;

        public void Write(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Length != StateSize) throw new ArgumentException("Length is invalid.", nameof(state));

            byte[] setConfigurationToOutput = {ConfigurationRegisterA, 0, 0};
            byte[] setState = {OutputPortRegisterA, state[0], state[1]};

            _i2CBus.Execute(_address, bus =>
            {
                bus.Write(setConfigurationToOutput);
                bus.Write(setState);
            });
        }

        public byte[] Read()
        {
            byte[] readState = {InputPortRegisterA};
            var buffer = new byte[StateSize];
            _i2CBus.Execute(_address, bus => bus.WriteRead(readState, buffer));

            return buffer;
        }

        public void SetPolarityInversion(byte[] polarityInversion)
        {
            if (polarityInversion == null) throw new ArgumentNullException(nameof(polarityInversion));
            if (polarityInversion.Length != StateSize) throw new ArgumentException("Length is invalid.", nameof(polarityInversion));

            byte[] polarityInversionRegister = {PolarityInversionRegisterA};
            _i2CBus.Execute(_address, bus => bus.WriteRead(polarityInversionRegister, polarityInversion));
        }
    }
}