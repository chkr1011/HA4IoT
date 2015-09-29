namespace CK.HomeAutomation.Hardware
{
    public class DummyOutputPort : DummyPort, IBinaryOutput
    {
        public new IBinaryOutput WithInvertedState()
        {
            return (IBinaryOutput) base.WithInvertedState();
        }
    }
}
