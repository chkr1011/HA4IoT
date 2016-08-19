using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Services.System
{
    public interface II2CBusService : IService
    {
        /// <summary>
        /// Executes the specified action providing the <see cref="II2CDevice"/> for the device with the specified address.
        /// This class is thread safe.
        /// </summary>
        /// <param name="address">The address of the device.</param>
        /// <param name="action">The action which sould be executed. The bus is locked while the action is being executed.</param>
        /// <param name="useCache">Indicates whether the device should be cached internally to improve performance (required if states are polled).</param>
        void Execute(I2CSlaveAddress address, Action<II2CDevice> action, bool useCache = true);
    }
}
