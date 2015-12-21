using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.GenericIOBoard
{
    public class IOBoardPort : IBinaryInput, IBinaryOutput
    {
        public IOBoardPort(int number, IOBoardControllerBase controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            Number = number;
            Controller = controller;

            controller.StateChanged += OnControllerStateChanged;
        }

        public int Number { get; }
        public bool InvertValue { get; set; }
        public IOBoardControllerBase Controller { get; }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public BinaryState Read()
        {
            return CoerceState(Controller.GetPortState(Number));
        }

        IBinaryInput IBinaryInput.WithInvertedState()
        {
            InvertValue = true;
            return this;
        }

        public void Write(BinaryState state, bool commit)
        {
            state = CoerceState(state);
            Controller.SetPortState(Number, state);

            if (commit)
            {
                Controller.CommitChanges();
            }
        }

        IBinaryOutput IBinaryOutput.WithInvertedState()
        {
            InvertValue = true;
            return this;
        }

        public IOBoardPort WithInvertedState()
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

        private void OnControllerStateChanged(object sender, IOBoardStateChangedEventArgs e)
        {
            bool oldState = e.OldState.GetBit(Number);
            bool newState = e.NewState.GetBit(Number);

            if (oldState == newState)
            {
                return;
            }

            BinaryState state = Read();
            StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(state));
        }
    }
}