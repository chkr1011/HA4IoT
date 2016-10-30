using System;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Features.Proxies;

namespace HA4IoT.Actuators
{
    public class PortBasedBinaryStateEndpoint : IBinaryStateEndpoint, ITurnOnFeatureProxy, ITurnOffFeatureProxy
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

            lock (_output)
            {
                bool commit = !parameters.Any(p => p is IsPartOfPartialUpdateParameter);
                _output.Write(BinaryState.High, commit);
            }
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            lock (_output)
            {
                bool commit = !parameters.Any(p => p is IsPartOfPartialUpdateParameter);
                _output.Write(BinaryState.Low, commit);
            }
        }

        public event EventHandler TurnedOn;
        public bool IsTurnedOn { get; private set; }
        public void TurnOn()
        {
            lock (_output)
            {
                if (IsTurnedOn)
                {
                    return;
                }

                _output.Write(BinaryState.High);
                IsTurnedOn = true;
                IsTurnedOff = false;
                TurnedOn?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler TurnedOff;
        public bool IsTurnedOff { get; private set; }
        public void TurnOff()
        {
            lock (_output)
            {
                if (IsTurnedOff)
                {
                    return;
                }

                _output.Write(BinaryState.Low);
                IsTurnedOn = false;
                IsTurnedOff = true;
                TurnedOff?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
