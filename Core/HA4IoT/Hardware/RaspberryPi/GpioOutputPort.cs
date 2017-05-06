using System;
using Windows.Devices.Gpio;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.RaspberryPi
{
    public sealed class GpioOutputPort : IBinaryOutput, IDisposable
    {
        private readonly object _syncRoot = new object();
        private readonly GpioPin _pin;

        private BinaryState _latestState;

        public GpioOutputPort(GpioPin pin)
        {
            _pin = pin ?? throw new ArgumentNullException(nameof(pin));
            _pin.SetDriveMode(GpioPinDriveMode.Output);
        }

        public bool IsStateInverted { get; set; }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;
        
        public BinaryState Read()
        {
            return _latestState;
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

        public void Dispose()
        {
            _pin?.Dispose();
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
