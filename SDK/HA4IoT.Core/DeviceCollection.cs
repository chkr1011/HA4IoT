using HA4IoT.Contracts.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HA4IoT.Core
{
    public class DeviceCollection
    {
        private readonly Dictionary<DeviceId, IDevice> _devices = new Dictionary<DeviceId, IDevice>();

        public void Add(IDevice device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));

            if (_devices.ContainsKey(device.Id))
            {
                throw new InvalidOperationException("Device with ID '" + device.Id + "' aready registered.");
            }

            _devices.Add(device.Id, device);
        }

        public TDevice Get<TDevice>(DeviceId id) where TDevice : IDevice
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            IDevice device;
            if (!_devices.TryGetValue(id, out device))
            {
                throw new InvalidOperationException("Device with ID '" + id + "' not registered.");
            }

            if (!(device is TDevice))
            {
                string message = string.Format(
                    "Device with ID '{0}' is registered but no '{1}' (is '{2}').",
                    id,
                    typeof(TDevice).Name,
                    device.GetType().Name);

                throw new InvalidOperationException(message);
            }

            return (TDevice)device;
        }

        public IList<TDevice> GetAll<TDevice>() where TDevice : IDevice
        {
            return _devices.Values.OfType<TDevice>().ToList();
        }
    }
}