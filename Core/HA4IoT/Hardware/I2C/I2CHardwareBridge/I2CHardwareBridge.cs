using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Devices;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.I2C.I2CHardwareBridge
{
    public class I2CHardwareBridge : IDevice
    {
        private readonly I2CSlaveAddress _address;
        private readonly II2CBusService _i2CBusService;

        public I2CHardwareBridge(string id, I2CSlaveAddress address, II2CBusService i2CBusService, ISchedulerService schedulerService)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            _address = address;
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));

            Id = id ?? throw new ArgumentNullException(nameof(id));
            DHT22Accessor = new DHT22Accessor(this, schedulerService);
        }

        public string Id { get; }

        public DHT22Accessor DHT22Accessor { get; }
        
        public void ExecuteCommand(I2CHardwareBridgeCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            command.Execute(_address, _i2CBusService);
        }
    }
}
