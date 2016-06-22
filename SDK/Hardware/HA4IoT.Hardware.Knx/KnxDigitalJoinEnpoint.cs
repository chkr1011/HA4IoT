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
            if (ValidationJoin(deviceId) && ValidationKnxController(knxController) )
            {
                _deviceId = deviceId;
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

        public bool ValidationJoin(string join)
        {
            Regex regexcheck = new Regex("([das])([0-9])");
            return regexcheck.IsMatch(join);
        }

        public bool ValidationKnxController(KnxController knxController)
        {
            if (knxController != null)
                return true;

            return false;
        }
    }
}
