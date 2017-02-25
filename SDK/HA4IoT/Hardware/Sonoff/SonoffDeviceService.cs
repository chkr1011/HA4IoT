using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Hardware.Sonoff
{
    public class SonoffDeviceService : ServiceBase
    {
        private readonly Dictionary<string, SonoffBinaryOutputAdapter> _adapters = new Dictionary<string, SonoffBinaryOutputAdapter>();
        private readonly IMqttService _mqttService;

        public SonoffDeviceService(IMqttService mqttService)
        {
            if (mqttService == null) throw new ArgumentNullException(nameof(mqttService));

            _mqttService = mqttService;
        }

        public IBinaryOutputAdapter GetBinaryOutputAdapter(string deviceName)
        {
            SonoffBinaryOutputAdapter adapter;
            if (!_adapters.TryGetValue(deviceName, out adapter))
            {
                adapter = new SonoffBinaryOutputAdapter(deviceName, _mqttService);
                _adapters.Add(deviceName, adapter);
            }

            return adapter;
        }
    }
}
