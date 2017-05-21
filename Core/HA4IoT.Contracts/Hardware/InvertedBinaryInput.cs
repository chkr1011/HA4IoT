using System;

namespace HA4IoT.Contracts.Hardware
{
    public class InvertedBinaryInput : IBinaryInput
    {
        private readonly IBinaryInput _binaryInput;

        public InvertedBinaryInput(IBinaryInput binaryInput)
        {
            _binaryInput = binaryInput ?? throw new ArgumentNullException(nameof(binaryInput));
            _binaryInput.StateChanged += ForwardStateChangedEvent;
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;
        
        public BinaryState Read()
        {
            return CoerceState(_binaryInput.Read());
        }

        protected static BinaryState CoerceState(BinaryState state)
        {
            return state == BinaryState.High ? BinaryState.Low : BinaryState.High;
        }

        private void ForwardStateChangedEvent(object sender, BinaryStateChangedEventArgs e)
        {
            StateChanged?.Invoke(sender, new BinaryStateChangedEventArgs(CoerceState(e.OldState), CoerceState(e.NewState)));
        }
    }
}
