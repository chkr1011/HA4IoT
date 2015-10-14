using System;

namespace CK.HomeAutomation.Hardware.I2CHardwareBridge
{
    public class I2CHardwareBridge
    {
        private readonly int _address;
        private readonly II2cBusAccessor _i2CBus;

        public I2CHardwareBridge(int address, II2cBusAccessor i2CBus)
        {
            if (i2CBus == null) throw new ArgumentNullException(nameof(i2CBus));

            _address = address;
            _i2CBus = i2CBus;
        }

        public void ExecuteCommand(I2CHardwareBridgeCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _i2CBus.Execute(_address, command.Execute, false);
        }
    }
}
