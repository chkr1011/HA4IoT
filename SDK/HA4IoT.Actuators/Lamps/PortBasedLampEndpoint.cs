using System;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public class PortBasedLampEndpoint : IBinaryStateEndpoint
    {
        private readonly IBinaryOutput _output;

        public PortBasedLampEndpoint(IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            _output = output;
        }

        public void TurnOn(params IParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            bool commit = !parameters.Any(p => p is DoNotCommitStateParameter);
            _output.Write(BinaryState.High, commit);
        }

        public void TurnOff(params IParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            bool commit = !parameters.Any(p => p is DoNotCommitStateParameter);
            _output.Write(BinaryState.High, commit);
        }
    }
}
