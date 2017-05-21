using System;
using System.Threading.Tasks;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Adapters.PortBased
{
    public class PortBasedRollerShutterAdapter : IRollerShutterAdapter
    {
        private readonly IBinaryOutput _powerOutput;
        private readonly IBinaryOutput _directionOutput;

        public PortBasedRollerShutterAdapter(IBinaryOutput powerOutput, IBinaryOutput directionOutput)
        {
            _powerOutput = powerOutput ?? throw new ArgumentNullException(nameof(powerOutput));
            _directionOutput = directionOutput ?? throw new ArgumentNullException(nameof(directionOutput));
        }

        public async Task SetState(AdapterRollerShutterState state, params IHardwareParameter[] parameters)
        {
            if (state == AdapterRollerShutterState.MoveUp)
            {
                await StopAndWait();
                _directionOutput.Write(BinaryState.Low);
                Start();
            }
            else if (state == AdapterRollerShutterState.MoveDown)
            {
                await StopAndWait();
                _directionOutput.Write(BinaryState.High);
                Start();
            }
            else
            {
                _powerOutput.Write(BinaryState.Low);

                // Ensure that the direction relay is not wasting energy.
                _directionOutput.Write(BinaryState.Low);
            }
        }

        private Task StopAndWait()
        {
            if (_powerOutput.Read() == BinaryState.Low)
            {
                return Task.FromResult(0);
            }

            _powerOutput.Write(BinaryState.Low);

            // Ensure that the relay is completely fallen off before switching the direction.
            return Task.Delay(100);
        }

        private void Start()
        {
            _powerOutput.Write(BinaryState.High);
        }
    }
}
