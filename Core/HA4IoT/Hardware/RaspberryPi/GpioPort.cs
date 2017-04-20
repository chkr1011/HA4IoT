using System;
using Windows.Devices.Gpio;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.RaspberryPi
{
    public sealed class GpioPort : IBinaryOutput, IBinaryInput, IDisposable
    {
        private readonly object _syncRoot = new object();
        private readonly GpioPin _pin;

        private BinaryState _latestState;

        public GpioPort(GpioPin pin)
        {
            _pin = pin ?? throw new ArgumentNullException(nameof(pin));

            if (pin.GetDriveMode() == GpioPinDriveMode.Input)
            {
                _pin.ValueChanged += HandleInterrupt;
            }
        }

        public bool IsStateInverted { get; set; }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        BinaryState IBinaryInput.Read()
        {
            lock (_syncRoot)
            {
                return ReadInternal(_pin.Read() == GpioPinValue.High ? BinaryState.High : BinaryState.Low);
            }
        }

        BinaryState IBinaryOutput.Read()
        {
            lock (_syncRoot)
            {
                return _latestState;
            }
        }

        public void Write(BinaryState state, WriteBinaryStateMode mode = WriteBinaryStateMode.Commit)
        {
            lock (_syncRoot)
            {
                if (state == _latestState)
                {
                    return;
                }

                var effectiveState = CoerceState(state);
                _pin.Write(effectiveState == BinaryState.High ? GpioPinValue.High : GpioPinValue.Low);

                var oldState = _latestState;
                _latestState = state;
                StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, state));
            }
        }

        private void HandleInterrupt(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            lock (_syncRoot)
            {
                ReadInternal(args.Edge == GpioPinEdge.RisingEdge ? BinaryState.High : BinaryState.Low);
            }
        }

        private BinaryState ReadInternal(BinaryState state)
        {
            var effectiveState = CoerceState(state);
            if (_latestState == effectiveState)
            {
                return _latestState;
            }
            
            var oldState = _latestState;
            _latestState = effectiveState;

            StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, effectiveState));

            return _latestState;
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

        public void Dispose()
        {
            if (_pin != null && _pin.GetDriveMode() == GpioPinDriveMode.Input)
            {
                _pin.ValueChanged -= HandleInterrupt;
            }

            _pin?.Dispose();
        }
    }
}
