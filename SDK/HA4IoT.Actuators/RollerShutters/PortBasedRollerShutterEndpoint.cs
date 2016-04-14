using System;
using System.Threading.Tasks;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.RollerShutters
{
    public class PortBasedRollerShutterEndpoint : IRollerShutterEndpoint
    {
        private readonly IBinaryOutput _powerOutput;
        private readonly IBinaryOutput _directionOutput;

        public PortBasedRollerShutterEndpoint(IBinaryOutput powerOutput, IBinaryOutput directionOutput)
        {
            if (powerOutput == null) throw new ArgumentNullException(nameof(powerOutput));
            if (directionOutput == null) throw new ArgumentNullException(nameof(directionOutput));

            _powerOutput = powerOutput;
            _directionOutput = directionOutput;
        }

        public void StartMoveUp(params IHardwareParameter[] parameters)
        {
            StopAndWait();
            _directionOutput.Write(BinaryState.Low);
            Start();
        }

        public void Stop(params IHardwareParameter[] parameters)
        {
            _powerOutput.Write(BinaryState.Low);

            // Ensure that the direction relay is not wasting energy.
            _directionOutput.Write(BinaryState.Low);
        }

        public void StartMoveDown(params IHardwareParameter[] parameters)
        {
            StopAndWait();
            _directionOutput.Write(BinaryState.High);
            Start();
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
