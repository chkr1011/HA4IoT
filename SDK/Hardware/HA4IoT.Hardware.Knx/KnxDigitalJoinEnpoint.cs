using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HA4IoT.Hardware.Knx
{
    public class KnxDigitalJoinEnpoint : IBinaryStateEndpoint
    {
        private readonly string _deviceId;
        private readonly KnxController _knxController;

        public KnxDigitalJoinEnpoint(string deviceId, KnxController knxController)
        {
            var regexcheck = new Regex("([s,d,a])([0-9])([=,0-9])");
            if (regexcheck.IsMatch(deviceId.Substring(0,3))) {
                _deviceId = deviceId;
            }

            if (knxController != null)
            {
                _knxController = knxController;
            }
        }

        public void TurnOn(params IHardwareParameter[] parameters)
        {
            _knxController.DigitalJoinOn(_deviceId);
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            _knxController.DigitalJoinOff(_deviceId);
        }
    }
}
