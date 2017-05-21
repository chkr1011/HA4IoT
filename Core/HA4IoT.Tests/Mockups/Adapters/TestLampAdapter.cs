using System.Threading.Tasks;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Mockups.Adapters
{
    public class TestLampAdapter : ILampAdapter
    {
        public bool SupportsColor { get; set; }
        public int ColorResolutionBits { get; set; }

        public int TurnOnCalledCount { get; set; }
        public int TurnOffCalledCount { get; set; }

        public Task SetState(AdapterPowerState powerState, AdapterColor color, params IHardwareParameter[] hardwareParameters)
        {
            if (powerState == AdapterPowerState.On)
            {
                TurnOnCalledCount++;
            }
            else if (powerState == AdapterPowerState.Off)
            {
                TurnOffCalledCount++;
            }

            return Task.FromResult(0);
        }
    }
}
