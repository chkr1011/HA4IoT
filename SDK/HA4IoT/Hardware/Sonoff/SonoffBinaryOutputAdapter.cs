using System;
using System.Text;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.Services;

namespace HA4IoT.Hardware.Sonoff
{
    public class SonoffBinaryOutputAdapter : IBinaryOutputAdapter
    {
        private readonly string _deviceName;
        private readonly IMqttService _mqttService;

        public SonoffBinaryOutputAdapter(string deviceName, IMqttService mqttService)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));
            if (mqttService == null) throw new ArgumentNullException(nameof(mqttService));

            _deviceName = deviceName;
            _mqttService = mqttService;
        }

        public void TurnOn(params IHardwareParameter[] parameters)
        {
            _mqttService.Publish("cmnd/" + _deviceName + "/power", Encoding.ASCII.GetBytes("ON"));
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            _mqttService.Publish("cmnd/" + _deviceName + "/power", Encoding.ASCII.GetBytes("OFF"));
        }
    }
}