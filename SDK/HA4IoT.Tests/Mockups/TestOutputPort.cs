using HA4IoT.Contracts.Hardware;
using HA4IoT.Tests.Hardware;

namespace HA4IoT.Tests.Mockups
{
    public class TestOutputPort : TestPort, IBinaryOutput
    {
        public new IBinaryOutput WithInvertedState(bool value = true)
        {
            return (IBinaryOutput) base.WithInvertedState(value);
        }
    }
}
