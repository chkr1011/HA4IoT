using System;

namespace HA4IoT.Hardware.Test
{
    public class TestPort
    {
        private readonly object _syncRoot = new object();
        private BinaryState _state = BinaryState.Low;
        private bool _stateIsInverted;

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public BinaryState GetInternalState()
        {
            lock (_syncRoot)
            {
                return _state;
            }
        }
        
        public void SetInternalState(BinaryState newState)
        {
            lock (_syncRoot)
            {
                _state = newState;
                StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(Read()));
            }
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
        
        public TestPort WithInvertedState()
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
