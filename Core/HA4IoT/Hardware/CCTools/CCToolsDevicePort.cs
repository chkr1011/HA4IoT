using System;
using System.Collections;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsDevicePort : IBinaryInput, IBinaryOutput
    {
        private readonly int _id;
        private readonly CCToolsDeviceBase _board;

        public CCToolsDevicePort(int id, CCToolsDeviceBase board)
        {
            _id = id;
            _board = board ?? throw new ArgumentNullException(nameof(board));

            board.StateChanged += OnBoardStateChanged;
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public bool IsStateInverted { get; set; }

        public BinaryState Read()
        {
            return CoerceState(_board.GetPortState(_id));
        }

        public void Write(BinaryState state, WriteBinaryStateMode mode)
        {
            var effectiveState = CoerceState(state);
            _board.SetPortState(_id, effectiveState);

            if (mode == WriteBinaryStateMode.Commit)
            {
                _board.CommitChanges();
            }
        }

        private BinaryState CoerceState(BinaryState state)
        {
            if (IsStateInverted)
            {
                return state == BinaryState.High ? BinaryState.Low : BinaryState.High;
            }

            return state;
        }

        private void OnBoardStateChanged(object sender, CCToolsDeviceStateChangedEventArgs e)
        {
            var stateChangedEvent = StateChanged;
            if (stateChangedEvent == null)
            {
                return;
            }

            var oldState = new BitArray(e.OldState).Get(_id);
            var newState = new BitArray(e.NewState).Get(_id);

            if (oldState == newState)
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