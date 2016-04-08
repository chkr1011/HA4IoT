using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public class I2CHardwareBridge : IDevice
    {
        private readonly I2CSlaveAddress _address;
        private readonly II2CBus _i2CBus;

        public I2CHardwareBridge(DeviceId id, I2CSlaveAddress address, II2CBus i2cBus, IHomeAutomationTimer timer)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (i2cBus == null) throw new ArgumentNullException(nameof(i2cBus));
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            Id = id;
            _address = address;
            _i2CBus = i2cBus;

            DHT22Accessor = new DHT22Accessor(this, timer);
        }

        public DeviceId Id { get; }
        public DHT22Accessor DHT22Accessor { get; }

        public void HandleApiCommand(IApiContext apiContext)
        {
        }

        public void HandleApiRequest(IApiContext apiContext)
        {
        }

        public void ExecuteCommand(I2CHardwareBridgeCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _i2CBus.Execute(_address, command.Execute, false);
        }
    }
}
