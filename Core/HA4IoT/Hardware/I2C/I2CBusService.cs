using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Hardware.I2C
{
    public class I2CBusService : ServiceBase, II2CBusService
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<int, I2cDevice> _deviceCache = new Dictionary<int, I2cDevice>();
        private readonly string _i2CBusId;
        private readonly ILogger _log;

        public I2CBusService(ILogService logService, IScriptingService scriptingService)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));
            _log = logService?.CreatePublisher(nameof(I2CBusService)) ?? throw new ArgumentNullException(nameof(logService));

            _i2CBusId = GetI2CBusId();

            scriptingService.RegisterScriptProxy(s => new I2CBusScriptProxy(this));
        }

        public II2CTransferResult Write(I2CSlaveAddress address, byte[] buffer, bool useCache = true)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            return Execute(address, d => d.WritePartial(buffer), useCache);
        }

        public II2CTransferResult Read(I2CSlaveAddress address, byte[] buffer, bool useCache = true)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            return Execute(address, d => d.ReadPartial(buffer), useCache);
        }

        public II2CTransferResult WriteRead(I2CSlaveAddress address, byte[] writeBuffer, byte[] readBuffer, bool useCache = true)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));

            return Execute(address, d => d.WriteReadPartial(writeBuffer, readBuffer), useCache);
        }

        private II2CTransferResult Execute(I2CSlaveAddress address, Func<I2cDevice, I2cTransferResult> action, bool useCache = true)
        {
            lock (_syncRoot)
            {
                I2cDevice device = null;
                try
                {
                    device = GetI2CDevice(address.Value, useCache);
                    var result = action(device);

                    if (result.Status != I2cTransferStatus.FullTransfer)
                    {
                        _log.Warning($"Transfer failed. Address={address.Value} Status={result.Status} TransferredBytes={result.BytesTransferred}");
                    }

                    return WrapResult(result);
                }
                catch (Exception exception)
                {
                    // Ensure that the application will not crash if some devices are currently not available etc.
                    _log.Warning(exception, $"Error while accessing I2C device with address {address}.");
                    return new I2CTransferResult(I2CTransferStatus.UnknownError, 0);
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

        private static II2CTransferResult WrapResult(I2cTransferResult result)
        {
            var status = I2CTransferStatus.UnknownError;
            switch (result.Status)
            {
                case I2cTransferStatus.FullTransfer:
                    {
                        status = I2CTransferStatus.FullTransfer;
                        break;
                    }

                case I2cTransferStatus.PartialTransfer:
                    {
                        status = I2CTransferStatus.PartialTransfer;
                        break;
                    }

                case I2cTransferStatus.ClockStretchTimeout:
                    {
                        status = I2CTransferStatus.ClockStretchTimeout;
                        break;
                    }

                case I2cTransferStatus.SlaveAddressNotAcknowledged:
                    {
                        status = I2CTransferStatus.SlaveAddressNotAcknowledged;
                        break;
                    }
            }

            return new I2CTransferResult(status, (int)result.BytesTransferred);
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
                var settings = new I2cConnectionSettings(address)
                {
                    BusSpeed = I2cBusSpeed.StandardMode,
                    SharingMode = I2cSharingMode.Exclusive
                };

                device = I2cDevice.FromIdAsync(_i2CBusId, settings).GetAwaiter().GetResult();
                if (useCache)
                {
                    _deviceCache.Add(address, device);
                }
            }

            return device;
        }

        private string GetI2CBusId()
        {
            var deviceSelector = I2cDevice.GetDeviceSelector();

            var deviceInformation = DeviceInformation.FindAllAsync(deviceSelector).GetAwaiter().GetResult();
            if (deviceInformation.Count == 0)
            {
                _log.Warning("No I2C bus found.");
                // TODO: Allow local controller to replace this. Then throw exception again.
                //throw new InvalidOperationException("I2C bus not found.");
                return null;
            }

            return deviceInformation.First().Id;
        }
    }
}