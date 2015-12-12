using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public class I2CHardwareBridge
    {
        private readonly I2CSlaveAddress _address;
        private readonly II2CBus _i2CBus;

        public I2CHardwareBridge(I2CSlaveAddress address, II2CBus i2CBus)
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
