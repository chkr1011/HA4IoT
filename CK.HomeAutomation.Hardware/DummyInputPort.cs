namespace CK.HomeAutomation.Hardware
{
    public class DummyInputPort : DummyPort, IBinaryInput
    {
        public new IBinaryInput WithInvertedState()
        {
            return (IBinaryInput)base.WithInvertedState();
        }
    }
}
