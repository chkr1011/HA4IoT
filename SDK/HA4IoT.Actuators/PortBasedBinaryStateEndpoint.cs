using System;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public class PortBasedBinaryStateEndpoint : IBinaryStateEndpoint
    {
        private readonly IBinaryOutput _output;

        public PortBasedBinaryStateEndpoint(IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            _output = output;
        }

        public void TurnOn(params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            bool commit = !parameters.Any(p => p is IsPartOfPartialUpdateParameter);
            _output.Write(BinaryState.High, commit);
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            bool commit = !parameters.Any(p => p is IsPartOfPartialUpdateParameter);
            _output.Write(BinaryState.Low, commit);
        }
    }
}
