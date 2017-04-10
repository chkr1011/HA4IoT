using System;
using Windows.Devices.Gpio;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.RaspberryPi
{
    public class GpioPort : IBinaryOutput, IBinaryInput
    {
        private readonly object _syncRoot = new object();

        private bool _isStateInverted;
        private BinaryState _previousState;

        public GpioPort(GpioPin pin)
        {
            if (pin == null) throw new ArgumentNullException(nameof(pin));

            Pin = pin;
        }

        public GpioPin Pin { get; }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        IBinaryInput IBinaryInput.WithInvertedState(bool value)
        {
            _isStateInverted = value;
            return this;
        }

        BinaryState IBinaryInput.Read()
        {
            lock (_syncRoot)
            {
                BinaryState currentState = CoerceState(Pin.Read() == GpioPinValue.High ? BinaryState.High : BinaryState.Low);
                if (currentState != _previousState)
                {
                    var oldState = _previousState;

                    _previousState = currentState;
                    StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, currentState));
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

                var oldState = _previousState;
                _previousState = state;
                StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(_previousState, state));
            }
        }

        BinaryState IBinaryOutput.Read()
        {
            lock (_syncRoot)
            {
                return CoerceState(Pin.Read() == GpioPinValue.High ? BinaryState.High : BinaryState.Low);
            }
        }

        IBinaryOutput IBinaryOutput.WithInvertedState(bool isInverted)
        {
            _isStateInverted = isInverted;
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
