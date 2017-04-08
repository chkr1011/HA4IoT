using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Mockups.Adapters
{
    public class TestFanAdapter : IFanAdapter
    {
        public int MaxLevel { get; set; }

        public int CurrentLevel { get; set; }

        public void SetState(int level, params IHardwareParameter[] parameters)
        {
            CurrentLevel = level;
        }
    }
}
