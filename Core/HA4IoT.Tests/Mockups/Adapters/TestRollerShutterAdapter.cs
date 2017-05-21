using System.Threading.Tasks;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Mockups.Adapters
{
    public class TestRollerShutterAdapter : IRollerShutterAdapter
    {
        public int StartMoveUpCalledCount { get; set; }

        public int StopCalledCount { get; set; }

        public int StartMoveDownCalledCount { get; set; }

        public Task SetState(AdapterRollerShutterState state, params IHardwareParameter[] parameters)
        {
            if (state == AdapterRollerShutterState.Stop) StopCalledCount++;
            if (state == AdapterRollerShutterState.MoveUp) StartMoveUpCalledCount++;
            if (state == AdapterRollerShutterState.MoveDown) StartMoveDownCalledCount++;

            return Task.FromResult(0);
        }
    }
}
