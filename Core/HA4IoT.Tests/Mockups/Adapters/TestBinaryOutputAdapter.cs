using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Mockups.Adapters
{
    public class TestBinaryOutputAdapter : IBinaryOutputAdapter
    {
        public int TurnOnCalledCount { get; set; }
        public int TurnOffCalledCount { get; set; }
        
        public void SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            if (powerState == AdapterPowerState.On) TurnOnCalledCount++;
            if (powerState == AdapterPowerState.Off) TurnOffCalledCount++;
        }
    }
}
