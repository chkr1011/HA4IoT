namespace CK.HomeAutomation.Hardware.Test
{
    public class TestInputPort : TestPort, IBinaryInput
    {
        public new IBinaryInput WithInvertedState()
        {
            return (IBinaryInput) base.WithInvertedState();
        }
    }
}
