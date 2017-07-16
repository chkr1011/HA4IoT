using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Components.Adapters;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Hardware.Drivers.Sonoff
{
    public class SonoffDeviceService : ServiceBase
    {
        private readonly Dictionary<string, SonoffBinaryOutputAdapter> _adapters = new Dictionary<string, SonoffBinaryOutputAdapter>();
        private readonly IDeviceMessageBrokerService _deviceMessageBrokerService;

        public SonoffDeviceService(IDeviceMessageBrokerService deviceMessageBrokerService)
        {
            if (deviceMessageBrokerService == null) throw new ArgumentNullException(nameof(deviceMessageBrokerService));

            _deviceMessageBrokerService = deviceMessageBrokerService;
        }

        public IBinaryOutputAdapter GetAdapterForPow(string deviceName)
        {
            SonoffBinaryOutputAdapter adapter;
            if (!_adapters.TryGetValue(deviceName, out adapter))
            {
                adapter = new SonoffBinaryOutputAdapter($"cmnd/{deviceName}/power", _deviceMessageBrokerService);
                _adapters.Add(deviceName, adapter);
            }

            return adapter;
        }

        public IBinaryOutputAdapter GetAdapterForDualRelay1(string deviceName)
        {
            SonoffBinaryOutputAdapter adapter;
            if (!_adapters.TryGetValue(deviceName, out adapter))
            {
                adapter = new SonoffBinaryOutputAdapter($"cmnd/{deviceName}/power1", _deviceMessageBrokerService);
                _adapters.Add(deviceName, adapter);
            }

            return adapter;
        }

        public IBinaryOutputAdapter GetAdapterForDualRelay2(string deviceName)
        {
            SonoffBinaryOutputAdapter adapter;
            if (!_adapters.TryGetValue(deviceName, out adapter))
            {
                adapter = new SonoffBinaryOutputAdapter($"cmnd/{deviceName}/power2", _deviceMessageBrokerService);
                _adapters.Add(deviceName, adapter);
            }

            return adapter;
        }
    }
}
