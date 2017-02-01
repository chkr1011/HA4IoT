using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Mockups
{
    public class TestRollerShutterEndpoint : IRollerShutterAdapter
    {
        public int StartMoveUpCalledCount { get; set; }

        public int StopCalledCount { get; set; }

        public int StartMoveDownCalledCount { get; set; }

        public void StartMoveUp(params IHardwareParameter[] parameters)
        {
            StartMoveUpCalledCount++;
        }

        public void Stop(params IHardwareParameter[] parameters)
        {
            StopCalledCount ++;
        }

        public void StartMoveDown(params IHardwareParameter[] parameters)
        {
            StartMoveDownCalledCount++;
        }
    }
}
