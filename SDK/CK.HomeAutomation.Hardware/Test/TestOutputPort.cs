namespace CK.HomeAutomation.Hardware.Test
{
    public class TestOutputPort : TestPort, IBinaryOutput
    {
        public new IBinaryOutput WithInvertedState()
        {
            return (IBinaryOutput) base.WithInvertedState();
        }
    }
}
