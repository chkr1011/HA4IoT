using System.Threading.Tasks;
using HA4IoT.Contracts.Components.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Mockups.Adapters
{
    public class TestBinaryOutputAdapter : IBinaryOutputAdapter
    {
        public int TurnOnCalledCount { get; set; }
        public int TurnOffCalledCount { get; set; }
        
        public Task SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            if (powerState == AdapterPowerState.On) TurnOnCalledCount++;
            if (powerState == AdapterPowerState.Off) TurnOffCalledCount++;

            return Task.FromResult(0);
        }
    }
}
