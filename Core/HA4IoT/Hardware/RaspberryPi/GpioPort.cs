using System;
using Windows.Devices.Gpio;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.RaspberryPi
{
    public class GpioPort : IBinaryOutput, IBinaryInput
    {
        private readonly object _syncRoot = new object();
        private readonly GpioPin _pin;

        private BinaryState _previousState;

        public GpioPort(GpioPin pin)
        {
            _pin = pin ?? throw new ArgumentNullException(nameof(pin));
        }

        public bool IsStateInverted { get; set; }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        BinaryState IBinaryInput.Read()
        {
            lock (_syncRoot)
            {
                var currentState = CoerceState(_pin.Read() == GpioPinValue.High ? BinaryState.High : BinaryState.Low);
                if (currentState != _previousState)
                {
                    var oldState = _previousState;
                    _previousState = currentState;

                    StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, currentState));
                }

                return currentState;
            }
        }

        public void Write(BinaryState state, WriteBinaryStateMode mode = WriteBinaryStateMode.Commit)
        {
            lock (_syncRoot)
            {
                state = CoerceState(state);

                if (state == _previousState)
                {
                    return;
                }

                _pin.Write(state == BinaryState.High ? GpioPinValue.High : GpioPinValue.Low);

                var oldState = _previousState;
                _previousState = state;
                StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, state));
            }
        }

        BinaryState IBinaryOutput.Read()
        {
            lock (_syncRoot)
            {
                return CoerceState(_pin.Read() == GpioPinValue.High ? BinaryState.High : BinaryState.Low);
            }
        }

        private BinaryState CoerceState(BinaryState state)
        {
            if (!IsStateInverted)
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
