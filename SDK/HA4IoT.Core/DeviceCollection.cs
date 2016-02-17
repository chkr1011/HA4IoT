using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Core
{
    public class DeviceCollection : GenericControllerCollection<DeviceId, IDevice>
    {
        ////private readonly Dictionary<DeviceId, IDevice> _devices = new Dictionary<DeviceId, IDevice>();

        ////public void AddUnique(IDevice device)
        ////{
        ////    if (device == null) throw new ArgumentNullException(nameof(device));

        ////    if (_devices.ContainsKey(device.Id))
        ////    {
        ////        throw new InvalidOperationException("Device with ID '" + device.Id + "' aready registered.");
        ////    }

        ////    _devices.Add(device.Id, device);
        ////}


        ////public void AddOrUpdate(IDevice device)
        ////{
        ////    if (device == null) throw new ArgumentNullException(nameof(device));
            
        ////    _devices[device.Id] = device;
        ////}

        ////public TDevice Get<TDevice>() where TDevice : IDevice
        ////{
        ////    return (TDevice)_devices.Values.Single(d => d is TDevice);
        ////}

        ////public TDevice Get<TDevice>(DeviceId id) where TDevice : IDevice
        ////{
        ////    if (id == null) throw new ArgumentNullException(nameof(id));

        ////    IDevice device;
        ////    if (!_devices.TryGetValue(id, out device))
        ////    {
        ////        throw new InvalidOperationException("Device with ID '" + id + "' not registered.");
        ////    }

        ////    if (!(device is TDevice))
        ////    {
        ////        string message = string.Format(
        ////            "Device with ID '{0}' is registered but no '{1}' (is '{2}').",
        ////            id,
        ////            typeof(TDevice).Name,
        ////            device.GetType().Name);

        ////        throw new InvalidOperationException(message);
        ////    }

        ////    return (TDevice)device;
        ////}

        ////public IList<TDevice> GetAll<TDevice>() where TDevice : IDevice
        ////{
        ////    return _devices.Values.OfType<TDevice>().ToList();
        ////}

        ////public IList<IDevice> GetAll()
        ////{
        ////    return _devices.Values.ToList();
        ////}
    }
}