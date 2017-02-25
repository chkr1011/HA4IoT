using System;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Adapters
{
    public class PortBasedWindowAdapter : IWindowAdapter
    {
        private readonly IBinaryInput _fullOpenReedSwitch;
        private readonly IBinaryInput _tildOpenReedSwitch;

        public PortBasedWindowAdapter(IBinaryInput fullOpenReedSwitch, IBinaryInput tildOpenReedSwitch = null)
        {
            if (fullOpenReedSwitch == null) throw new ArgumentNullException(nameof(fullOpenReedSwitch));

            _fullOpenReedSwitch = fullOpenReedSwitch;
            _tildOpenReedSwitch = tildOpenReedSwitch;

            if (_tildOpenReedSwitch != null)
            {
                _tildOpenReedSwitch.StateChanged += (s, e) => Update();
            }

            _fullOpenReedSwitch.StateChanged += (s, e) => Update();

            Update();
        }

        public event EventHandler<WindowStateChangedEventArgs> StateChanged;

        private void Update()
        {
            var fullOpenReedSwitchState = _fullOpenReedSwitch.Read() == BinaryState.High
                ? ReedSwitchState.Closed
                : ReedSwitchState.Open;

            ReedSwitchState? tildOpenReedSwitchState = null;
            if (_tildOpenReedSwitch != null)
            {
                tildOpenReedSwitchState = _tildOpenReedSwitch.Read() == BinaryState.High
                    ? ReedSwitchState.Closed
                    : ReedSwitchState.Open;
            }

            StateChanged?.Invoke(this, new WindowStateChangedEventArgs(fullOpenReedSwitchState, tildOpenReedSwitchState));
        }
    }
}
