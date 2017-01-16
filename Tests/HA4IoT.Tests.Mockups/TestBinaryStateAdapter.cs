using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Mockups
{
    public class TestBinaryStateAdapter : IBinaryStateAdapter
    {
        public int TurnOnCalledCount { get; set; }
        public int TurnOffCalledCount { get; set; }

        public void TurnOn(params IHardwareParameter[] parameters)
        {
            TurnOnCalledCount++;
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            TurnOffCalledCount++;
        }
    }
}
