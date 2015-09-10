using System;

namespace CK.HomeAutomation.Hardware
{
    public class DummyPort : IBinaryInput, IBinaryOutput
    {
        private readonly object _syncRoot = new object();
        private BinaryState _state = BinaryState.Low;
        private bool _stateIsInverted;

        public BinaryState FakeState
        {
            get
            {
                lock (_syncRoot)
                {
                    return _state;
                }
            }
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public void FireStateChangedEvent()
        {
            StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(Read()));
        }

        public void Write(BinaryState state, bool commit = true)
        {
            lock (_syncRoot)
            {
                _state = CoerceState(state);
            }
        }

        public BinaryState Read()
        {
            lock (_syncRoot)
            {
                return CoerceState(_state);
            }
        }

        IBinaryOutput IBinaryOutput.WithInvertedState()
        {
            _stateIsInverted = true;
            return this;
        }

        IBinaryInput IBinaryInput.WithInvertedState()
        {
            _stateIsInverted = true;
            return this;
        }

        private BinaryState CoerceState(BinaryState state)
        {
            if (!_stateIsInverted)
            {
                return state;
            }

            return state == BinaryState.High ? BinaryState.Low : BinaryState.High;
        }
    }
}
