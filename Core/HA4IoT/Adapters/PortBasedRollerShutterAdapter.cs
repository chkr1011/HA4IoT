using System;
using System.Threading.Tasks;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Adapters
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

        public void SetState(AdapterRollerShutterState state, params IHardwareParameter[] parameters)
        {
            if (state == AdapterRollerShutterState.MoveUp)
            {
                StopAndWait();
                _directionOutput.Write(BinaryState.Low);
                Start();
            }
            else if (state == AdapterRollerShutterState.MoveDown)
            {
                StopAndWait();
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

        private void StopAndWait()
        {
            if (_powerOutput.Read() == BinaryState.Low)
            {
                return;
            }

            _powerOutput.Write(BinaryState.Low);

            // Ensure that the relay is completely fallen off before switching the direction.
            Task.Delay(25).Wait();
        }

        private void Start()
        {
            _powerOutput.Write(BinaryState.High);
        }
    }
}
