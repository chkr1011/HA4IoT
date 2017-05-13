using System;
using System.Threading;
using Windows.Devices.Gpio;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.RaspberryPi
{
    public sealed class GpioInputPort : IBinaryInput, IDisposable
    {
        public const int PollInterval = 25;

        private readonly object _syncRoot = new object();
        private readonly GpioPin _pin;
        // ReSharper disable once NotAccessedField.Local
        private readonly Timer _timer;

        private BinaryState _latestState;

        public GpioInputPort(GpioPin pin, GpioInputMonitoringMode mode = GpioInputMonitoringMode.Interrupt)
        {
            _pin = pin ?? throw new ArgumentNullException(nameof(pin));
            _pin.SetDriveMode(GpioPinDriveMode.Input);
            _latestState = ReadAndConvert();

            if (mode == GpioInputMonitoringMode.Polling)
            {
                _timer = new Timer(PollState, null, 0, Timeout.Infinite);
            }
            else if (mode == GpioInputMonitoringMode.Interrupt)
            {
                _pin.ValueChanged += HandleInterrupt;
            }
        }

        public bool IsStateInverted { get; set; }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public BinaryState Read()
        {
            return Update(ReadAndConvert());
        }

        public void Dispose()
        {
            _pin.ValueChanged -= HandleInterrupt;
            _pin?.Dispose();
        }

        private void HandleInterrupt(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            Log.Default.Verbose("Interrupt raised for GPIO" + _pin.PinNumber + ".");

            var newState1 = ReadAndConvert();
            //Task.Delay(PollInterval).Wait();
            //var newState2 = ReadAndConvert();

            //if (newState1 != newState2)
            //{
            //    return;
            //}

            Update(newState1);
        }

        private void PollState(object state)
        {
            try
            {
                Update(ReadAndConvert());
            }
            catch (Exception exception)
            {
                Log.Default.Error(exception, $"Error while polling input state of GPIO{_pin.PinNumber}.");
            }
            finally
            {
                _timer.Change(PollInterval, Timeout.Infinite);
            }
        }

        private BinaryState CoerceState(BinaryState state)
        {
            if (!IsStateInverted)
            {
                return state;
            }

            return state == BinaryState.High ? BinaryState.Low : BinaryState.High;
        }

        private BinaryState ReadAndConvert()
        {
            var gpioPinValue = _pin.Read();
            var state = gpioPinValue == GpioPinValue.High ? BinaryState.High : BinaryState.Low;
            return CoerceState(state);
        }

        private BinaryState Update(BinaryState newState)
        {
            BinaryState oldState;

            // Use double lock pattern here!
            if (_latestState == newState)
            {
                return _latestState;
            }

            lock (_syncRoot)
            {
                if (_latestState == newState)
                {
                    return _latestState;
                }

                oldState = _latestState;
                _latestState = newState;
            }

            try
            {
                StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, newState));
            }
            catch (Exception exception)
            {
                Log.Default.Error(exception, $"Error while reading input state of GPIO{_pin.PinNumber}.");
            }

            return newState;
        }
    }
}
