using System;
using System.Threading;
using Windows.Devices.Gpio;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.RaspberryPi
{
    public sealed class GpioInputPort : IBinaryInput, IDisposable
    {
        private const int PollInterval = 15;
        private const long DebounceTimeoutTicks = 1000;

        private readonly GpioPin _pin;
        // ReSharper disable once NotAccessedField.Local
        private readonly Timer _timer;

        private BinaryState _latestState;

        public GpioInputPort(GpioPin pin, GpioInputMonitoringMode mode = GpioInputMonitoringMode.Interrupt)
        {
            _pin = pin ?? throw new ArgumentNullException(nameof(pin));
            _pin.SetDriveMode(GpioPinDriveMode.Input);
            //_pin.DebounceTimeout = TimeSpan.FromTicks(DebounceTimeoutTicks);

            if (mode == GpioInputMonitoringMode.Polling)
            {
                _timer = new Timer(PollState, null, 0, Timeout.Infinite);
            }
            else if (mode == GpioInputMonitoringMode.Interrupt)
            {
                _pin.ValueChanged += HandleInterrupt;
            }

            _latestState = ReadAndConvert();
        }

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
            var newState = ReadAndConvert();

            Log.Default.Verbose("Interrupt raised for GPIO" + _pin.PinNumber + ".");
            Update(newState);
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

        private BinaryState ReadAndConvert()
        {
            return _pin.Read() == GpioPinValue.High ? BinaryState.High : BinaryState.Low;
        }

        private BinaryState Update(BinaryState newState)
        {
            var oldState = _latestState;

            if (oldState == newState)
            {
                return oldState;
            }

            _latestState = newState;

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
