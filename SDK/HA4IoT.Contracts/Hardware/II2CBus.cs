using System;
using Windows.Devices.I2c;

namespace HA4IoT.Contracts.Hardware
{
    public interface II2CBus
    {
        /// <summary>
        /// Executes the specified action providing the <see cref="I2cDevice"/> for the device with the specified address.
        /// This class is thread safe.
        /// </summary>
        /// <param name="address">The address of the device.</param>
        /// <param name="action">The action which sould be executed. The bus is locked while the action is being executed.</param>
        /// <param name="useCache">Indicates whether the <see cref="I2cDevice"/> with the specified address should be cached internally to improve performance (required if states are polled).</param>
        void Execute(I2CSlaveAddress address, Action<I2cDevice> action, bool useCache = true);
    }
}
