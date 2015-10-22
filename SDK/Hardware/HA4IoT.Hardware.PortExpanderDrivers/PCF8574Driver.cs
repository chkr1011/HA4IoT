using System;

namespace CK.HomeAutomation.Hardware.PortExpanderDrivers
{
    public class PCF8574Driver : IPortExpanderDriver
    {
        private readonly II2cBusAccessor _i2CBus;
        private readonly int _address;

        public PCF8574Driver(int address, II2cBusAccessor i2CBus)
        {
            if (i2CBus == null) throw new ArgumentNullException(nameof(i2CBus));

            _address = address;
            _i2CBus = i2CBus;
        }

        public int StateSize { get; } = 1;

        public void Write(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Length != StateSize) throw new ArgumentException("Length is invalid.", nameof(state));

            // TODO: Check error handling here if device is currently not available.

            _i2CBus.Execute(_address, bus => bus.Write(state));
        }

        public byte[] Read()
        {
            // TODO: Check error handling here if device is currently not available.

            var buffer = new byte[StateSize];
            _i2CBus.Execute(_address, bus => bus.Read(buffer));

            return buffer;
        }
    }
}