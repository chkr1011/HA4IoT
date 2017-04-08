using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsDevicePort : IBinaryInput, IBinaryOutput
    {
        public CCToolsDevicePort(int number, CCToolsDeviceBase board)
        {
            Number = number;
            Board = board ?? throw new ArgumentNullException(nameof(board));

            board.StateChanged += OnControllerStateChanged;
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public int Number { get; }
        public bool InvertValue { get; set; }
        public CCToolsDeviceBase Board { get; }

        public BinaryState Read()
        {
            return CoerceState(Board.GetPortState(Number));
        }

        IBinaryInput IBinaryInput.WithInvertedState(bool value)
        {
            InvertValue = value;
            return this;
        }

        public void Write(BinaryState state, bool commit)
        {
            state = CoerceState(state);
            Board.SetPortState(Number, state);

            if (commit)
            {
                Board.CommitChanges();
            }
        }

        IBinaryOutput IBinaryOutput.WithInvertedState(bool value)
        {
            InvertValue = true;
            return this;
        }

        public CCToolsDevicePort WithInvertedState()
        {
            InvertValue = true;
            return this;
        }

        private BinaryState CoerceState(BinaryState state)
        {
            if (InvertValue)
            {
                return state == BinaryState.High ? BinaryState.Low : BinaryState.High;
            }

            return state;
        }

        private void OnControllerStateChanged(object sender, CCToolsDeviceStateChangedEventArgs e)
        {
            var oldState = e.OldState.GetBit(Number);
            var newState = e.NewState.GetBit(Number);

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

            StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldBinaryState, newBinaryState));
        }
    }
}