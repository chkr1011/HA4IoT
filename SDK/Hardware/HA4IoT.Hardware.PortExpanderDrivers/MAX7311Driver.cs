using System;

namespace HA4IoT.Hardware.PortExpanderDrivers
{
    public class MAX7311Driver : IPortExpanderDriver
    {
        private readonly II2cBusAccessor _i2CBus;
        private readonly int _address;

        private const byte InputPortRegisterA = 0;
        private const byte OutputPortRegisterA = 2;
        private const byte PolarityInversionRegisterA = 4;
        private const byte ConfigurationRegisterA = 6;
        
        public MAX7311Driver(int address, II2cBusAccessor i2CBus)
        {
            if (i2CBus == null) throw new ArgumentNullException(nameof(i2CBus));

            _address = address;
            _i2CBus = i2CBus;
        }

        public int StateSize { get; } = 2;

        public void Write(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Length != StateSize) throw new ArgumentException("Length is invalid.", nameof(state));

            // TODO: Check error handling here if device is currently not available.

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
            // TODO: Check error handling here if device is currently not available.

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