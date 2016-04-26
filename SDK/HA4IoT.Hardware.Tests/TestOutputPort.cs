using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.Tests
{
    public class TestOutputPort : TestPort, IBinaryOutput
    {
        public new IBinaryOutput WithInvertedState(bool value = true)
        {
            return (IBinaryOutput) base.WithInvertedState(value);
        }
    }
}
