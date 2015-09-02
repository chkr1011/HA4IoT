using System;
using Windows.Devices.Gpio;
using CK.HomeAutomation.Core;

namespace CK.HomeAutomation.Hardware.Pi2
{
    public class Pi2Port : IBinaryOutput, IBinaryInput
    {
        private BinaryState _previousState;
        private bool _isStateInverted;

        public Pi2Port(GpioPin pin)
        {
            if (pin == null) throw new ArgumentNullException(nameof(pin));

            Pin = pin;
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public GpioPin Pin { get; }

        public void Write(BinaryState state, bool commit = true)
        {
            state = CoerceState(state);

            if (state == _previousState)
            {
                return;
            }

            Pin.Write(state == BinaryState.High ? GpioPinValue.High : GpioPinValue.Low);
            _previousState = state;
            StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(state));
        }

        BinaryState IBinaryOutput.Read()
        {
            return CoerceState(Pin.Read() == GpioPinValue.High ? BinaryState.High : BinaryState.Low);
        }

        IBinaryInput IBinaryInput.WithInvertedState()
        {
            _isStateInverted = true;
            return this;
        }

        BinaryState IBinaryInput.Read()
        {
            BinaryState currentState = CoerceState(Pin.Read() == GpioPinValue.High ? BinaryState.High : BinaryState.Low);
            if (currentState != _previousState)
            {
                _previousState = currentState;
                StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(currentState));
            }

            return currentState;
        }

        IBinaryOutput IBinaryOutput.WithInvertedState()
        {
            _isStateInverted = true;
            return this;
        }

        private BinaryState CoerceState(BinaryState state)
        {
            if (!_isStateInverted)
            {
                return state;
            }

            if (state == BinaryState.High)
            {
                return BinaryState.Low;
            }

            return BinaryState.High;
        }
    }
}
