using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Mockups
{
    public class TestRollerShutterEndpoint : IRollerShutterEndpoint
    {
        public void StartMoveUp(params IHardwareParameter[] parameters)
        {
        }

        public void Stop(params IHardwareParameter[] parameters)
        {
        }

        public void StartMoveDown(params IHardwareParameter[] parameters)
        {
        }
    }
}
