using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Hardware.I2C
{
    public class BuiltInI2CBusService : ServiceBase, II2CBusService
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<int, I2CDeviceWrapper> _deviceCache = new Dictionary<int, I2CDeviceWrapper>();
        private readonly string _i2CBusId;
        private readonly ILogger _log;

        public BuiltInI2CBusService(ILogService logService)
        {
            _log = logService?.CreatePublisher(nameof(BuiltInI2CBusService)) ?? throw new ArgumentNullException(nameof(logService));

            var deviceSelector = I2cDevice.GetDeviceSelector();
            
            var deviceInformation = DeviceInformation.FindAllAsync(deviceSelector).GetAwaiter().GetResult();
            if (deviceInformation.Count == 0)
            {
                _log.Warning("No I2C bus found.");
                // TODO: Allow local controller to replace this. Then throw exception again.
                //throw new InvalidOperationException("I2C bus not found.");
                return;
            }

            _i2CBusId = deviceInformation.First().Id;
        }
        
        public void Execute(I2CSlaveAddress address, Action<II2CDevice> action, bool useCache = true)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            lock (_syncRoot)
            {
                I2CDeviceWrapper device = null;
                try
                {
                    device = GetI2CDevice(address.Value, useCache);
                    action(device);
                }
                catch (Exception exception)
                {
                    // Ensure that the application will not crash if some devices are currently not available etc.
                    _log.Warning(exception, $"Error while accessing I2C device with address {address}.");
                }
                finally
                {
                    if (device != null && !useCache)
                    {
                        device.Dispose();
                    }
                }
            }
        }

        private I2CDeviceWrapper GetI2CDevice(int address, bool useCache)
        {
            // TODO: The cache is required because using the I2cDevice.FromIdAsync method every time tooks a very long time.
            // Polling the inputs can take up to 300ms (for all) which is too slow (some very short pressed buttons are missed).
            // The Arduino Nano T&H bridge does not work correctly when reusing the device. More investigation is required!
            // At this time, the cache can be disabled for certain devices.
            I2CDeviceWrapper device;
            if (!useCache || !_deviceCache.TryGetValue(address, out device))
            {
                var settings = new I2cConnectionSettings(address)
                {
                    BusSpeed = I2cBusSpeed.StandardMode,
                    SharingMode = I2cSharingMode.Exclusive
                };

                device = new I2CDeviceWrapper(I2cDevice.FromIdAsync(_i2CBusId, settings).GetAwaiter().GetResult());

                if (useCache)
                {
                    _deviceCache.Add(address, device);
                }
            }

            return device;
        }
    }
}