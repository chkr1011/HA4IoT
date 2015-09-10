using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware
{
    public class I2CBus
    {
        private readonly Dictionary<int, I2cDevice> _deviceCache = new Dictionary<int, I2cDevice>();
        private readonly string _i2CBusId;
        private readonly INotificationHandler _notificationHandler;
        private readonly object _syncRoot = new object();

        public I2CBus(INotificationHandler notificationHandler)
        {
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));

            _notificationHandler = notificationHandler;

            string deviceSelector = I2cDevice.GetDeviceSelector();
            
            DeviceInformationCollection deviceInformation = DeviceInformation.FindAllAsync(deviceSelector).AsTask().Result;
            if (deviceInformation.Count == 0)
            {
                throw new InvalidOperationException("I2C bus not found.");
            }

            _i2CBusId = deviceInformation.First().Id;
        }

        public void Execute(int address, Action<I2cDevice> action, bool useCache = true)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            lock (_syncRoot)
            {
                I2cDevice device = null;
                try
                {
                    device = GetI2CDevice(address, useCache);
                    action(device);
                }
                catch (Exception exception)
                {
                    // Ensure that the application will not crash if some devices are currently not available etc.
                    _notificationHandler.PublishFrom(this, NotificationType.Warning,
                        "Error while accessing I2C device with address " + address + ". " + exception.Message);
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

        private I2cDevice GetI2CDevice(int address, bool useCache)
        {
            // TODO: The cache is required because using the I2cDevice.FromIdAsync method every time tooks a very long time.
            // Polling the inputs can take up to 300ms (for all) which is too slow (some very short pressed buttons are missed).
            // The Arduino Nano T&H bridge does not work correctly when reusing the device. More investigation is required!
            // At this time, the cache can be disabled for certain devices.
            I2cDevice device;
            if (!useCache || !_deviceCache.TryGetValue(address, out device))
            {
                var settings = new I2cConnectionSettings(address);
                settings.BusSpeed = I2cBusSpeed.StandardMode;
                settings.SharingMode = I2cSharingMode.Exclusive;

                device = I2cDevice.FromIdAsync(_i2CBusId, settings).AsTask().Result;

                if (useCache)
                {
                    _deviceCache.Add(address, device);
                }
            }

            return device;
        }
    }
}