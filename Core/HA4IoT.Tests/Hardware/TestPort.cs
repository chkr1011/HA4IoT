using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Hardware
{
    public class TestPort : IBinaryInput, IBinaryOutput
    {
        private readonly object _syncRoot = new object();
        private BinaryState _state = BinaryState.Low;

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public bool IsStateInverted { get; set; }

        public BinaryState GetInternalState()
        {
            lock (_syncRoot)
            {
                return _state;
            }
        }
        
        public void SetInternalState(BinaryState state)
        {
            lock (_syncRoot)
            {
                var oldState = _state;
                _state = state;
                StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, state));
            }
        }

        public void Write(BinaryState state, WriteBinaryStateMode mode = WriteBinaryStateMode.Commit)
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
        
        private BinaryState CoerceState(BinaryState state)
        {
            if (!IsStateInverted)
            {
                return state;
            }

            return state == BinaryState.High ? BinaryState.Low : BinaryState.High;
        }
    }
}
