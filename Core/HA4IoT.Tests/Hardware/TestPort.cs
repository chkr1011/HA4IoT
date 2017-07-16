using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Hardware
{
    public class TestPort : IBinaryOutput
    {
        private BinaryState _state;

        public TestPort(BinaryState initialState)
        {
            _state = initialState;
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public void Write(BinaryState state, WriteBinaryStateMode mode = WriteBinaryStateMode.Commit)
        {
            if (state == _state)
            {
                return;
            }

            var oldState = _state;
            _state = state;

            StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, state));
        }

        public BinaryState Read()
        {
            return _state;
        }
    }
}
