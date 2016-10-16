using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using System;
using System.Text.RegularExpressions;

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

            if (!ValidationJoin(identifier)) throw new ArgumentException("identifier is in a wrong format");

            _identifier = identifier;
            _knxController = knxController;
        }

        public void TurnOn(params IHardwareParameter[] parameters)
        {
            _knxController.SendDigitalJoinOn(_identifier);
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            _knxController.SendDigitalJoinOff(_identifier);
        }

        private bool ValidationJoin(string join)
        {
            return new Regex("([das])([0-9])").IsMatch(join);
        }
    }
}
