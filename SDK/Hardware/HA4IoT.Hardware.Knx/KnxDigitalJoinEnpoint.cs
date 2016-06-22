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
        private readonly string _identifier;
        private readonly KnxController _knxController;

        public KnxDigitalJoinEnpoint(string identifier, KnxController knxController)
        {
            if (identifier == null) throw new ArgumentNullException(nameof(identifier));
            if (knxController == null) throw new ArgumentNullException(nameof(knxController));

            if (ValidationJoin(identifier))
            {
                _identifier = identifier;
                _knxController = knxController;
            }
        }

        public void TurnOn(params IHardwareParameter[] parameters)
        {
            _knxController.DigitalJoinOn(_identifier);
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            _knxController.DigitalJoinOff(_deviceId);
        }

        private bool ValidationJoin(string join)
        {
            Regex regexcheck = new Regex("([das])([0-9])");
            return regexcheck.IsMatch(join);
        }
        
    }
}
