using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.Test
{
    public class TestOutputPort : TestPort, IBinaryOutput
    {
        public new IBinaryOutput WithInvertedState()
        {
            return (IBinaryOutput) base.WithInvertedState();
        }
    }
}
