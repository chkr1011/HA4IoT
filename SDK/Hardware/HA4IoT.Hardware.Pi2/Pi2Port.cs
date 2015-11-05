using System;
using Windows.Devices.Gpio;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.Pi2
{
    public class Pi2Port : IBinaryOutput, IBinaryInput
    {
        private readonly object _syncRoot = new object();
        private bool _isStateInverted;
        private BinaryState _previousState;

        public Pi2Port(GpioPin pin)
        {
            if (pin == null) throw new ArgumentNullException(nameof(pin));

            Pin = pin;
        }

        public GpioPin Pin { get; }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        IBinaryInput IBinaryInput.WithInvertedState()
        {
            _isStateInverted = true;
            return this;
        }

        BinaryState IBinaryInput.Read()
        {
            lock (_syncRoot)
            {
                BinaryState currentState = CoerceState(Pin.Read() == GpioPinValue.High ? BinaryState.High : BinaryState.Low);
                if (currentState != _previousState)
                {
                    _previousState = currentState;
                    StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(currentState));
                }

                return currentState;
            }
        }

        public void Write(BinaryState state, bool commit = true)
        {
            lock (_syncRoot)
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
        }

        BinaryState IBinaryOutput.Read()
        {
            lock (_syncRoot)
            {
                return CoerceState(Pin.Read() == GpioPinValue.High ? BinaryState.High : BinaryState.Low);
            }
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
