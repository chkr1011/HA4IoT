using HA4IoT.Contracts.Hardware;
using System;
using System.Text.RegularExpressions;
using HA4IoT.Contracts.Adapters;

namespace HA4IoT.Hardware.Knx
{
    public class KnxDigitalJoinEnpoint : IBinaryOutputAdapter
    {
        private readonly string _identifier;
        private readonly KnxController _knxController;

        public KnxDigitalJoinEnpoint(string identifier, KnxController knxController)
        {
            if (identifier == null) throw new ArgumentNullException(nameof(identifier));
            if (!ValidationJoin(identifier)) throw new ArgumentException("Identifier is in a wrong format");

            _identifier = identifier;
            _knxController = knxController ?? throw new ArgumentNullException(nameof(knxController));
        }

        public void SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            if (powerState == AdapterPowerState.On)
            {
                _knxController.SendDigitalJoinOn(_identifier);
            }
            else
            {
                _knxController.SendDigitalJoinOff(_identifier);
            }
        }

        private bool ValidationJoin(string join)
        {
            return new Regex("([das])([0-9])").IsMatch(join);
        }
    }
}
