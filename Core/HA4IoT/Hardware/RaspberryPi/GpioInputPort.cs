using System;
using Windows.Devices.Gpio;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.RaspberryPi
{
    public sealed class GpioInputPort : IBinaryInput, IDisposable
    {
        //private const int PollInterval = 15;
        //private readonly Timer _timer;

        private readonly object _syncRoot = new object();
        private readonly GpioPin _pin;
        
        private BinaryState _latestState;

        public GpioInputPort(GpioPin pin)
        {
            _pin = pin ?? throw new ArgumentNullException(nameof(pin));
            _pin.SetDriveMode(GpioPinDriveMode.Input);

            //_timer = new Timer(HandleTimerCallback, null, 0, Timeout.Infinite);
            _pin.ValueChanged += HandleInterrupt;
        }

        public bool IsStateInverted { get; set; }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public BinaryState Read()
        {
            lock (_syncRoot)
            {
                return ReadInternal(_pin.Read() == GpioPinValue.High ? BinaryState.High : BinaryState.Low);
            }
        }

        public void Dispose()
        {
            _pin.ValueChanged -= HandleInterrupt;
            _pin?.Dispose();
        }

        private void HandleInterrupt(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            lock (_syncRoot)
            {
                ReadInternal(args.Edge == GpioPinEdge.RisingEdge ? BinaryState.High : BinaryState.Low);
            }
        }

        ////private void HandleTimerCallback(object sender)
        ////{
        ////    lock (_syncRoot)
        ////    {
        ////        try
        ////        {
        ////            ReadAsInput();
        ////        }
        ////        catch (Exception exception)
        ////        {
        ////            Log.Default.Error(exception, $"Error while processing interrupt of GPIO port ({_pin.PinNumber}).");
        ////        }
        ////        finally
        ////        {
        ////            _timer.Change(PollInterval, 0);
        ////        }
        ////    }
        ////}

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
    }
}
