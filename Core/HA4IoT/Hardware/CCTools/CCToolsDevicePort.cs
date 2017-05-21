using System;
using System.Collections;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsDevicePort : IBinaryOutput
    {
        private readonly int _id;
        private readonly CCToolsDeviceBase _board;

        public CCToolsDevicePort(int id, CCToolsDeviceBase board)
        {
            _id = id;
            _board = board ?? throw new ArgumentNullException(nameof(board));
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;
        
        public BinaryState Read()
        {
            return _board.GetPortState(_id);
        }

        public void Write(BinaryState state, WriteBinaryStateMode mode)
        {
            _board.SetPortState(_id, state);

            if (mode == WriteBinaryStateMode.Commit)
            {
                _board.CommitChanges();
            }
        }

        public void OnBoardStateChanged(byte[] oldState, byte[] newState)
        {
            var stateChangedEvent = StateChanged;
            if (stateChangedEvent == null)
            {
                return;
            }

            var oldPinState = new BitArray(oldState).Get(_id);
            var newPinState = new BitArray(newState).Get(_id);

            if (oldPinState == newPinState)
            {
                return;
            }

            var newBinaryState = Read();
            var oldBinaryState = BinaryState.High;
            if (newBinaryState == BinaryState.High)
            {
                oldBinaryState = BinaryState.Low;
            }

            stateChangedEvent.Invoke(this, new BinaryStateChangedEventArgs(oldBinaryState, newBinaryState));
        }
    }
}