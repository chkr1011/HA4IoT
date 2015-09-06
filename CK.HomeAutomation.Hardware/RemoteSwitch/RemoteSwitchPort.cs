using System;
using CK.HomeAutomation.Core;
using CK.HomeAutomation.Hardware.Drivers;

namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class RemoteSwitchPort : IBinaryOutput
    {
        private readonly RemoteSwitchCode _onCode;
        private readonly RemoteSwitchCode _offCode;
        private readonly RemoteSwitchBridgeDriver _bridge;
        private BinaryState _state;
        
        public RemoteSwitchPort(int id, RemoteSwitchCode onCode, RemoteSwitchCode offCode, RemoteSwitchBridgeDriver bridge, HomeAutomationTimer timer)
        {
            if (onCode == null) throw new ArgumentNullException(nameof(onCode));
            if (offCode == null) throw new ArgumentNullException(nameof(offCode));
            if (bridge == null) throw new ArgumentNullException(nameof(bridge));

            _onCode = onCode;
            _offCode = offCode;
            _bridge = bridge;

            // Ensure that the state of the remote switch is restored if the original remote is used
            // or the switch has been removed from the socket and plugged in at another place.
            timer.Every(TimeSpan.FromSeconds(5)).Do(() => Write(_state));
        }

        public void Write(BinaryState state, bool commit = true)
        {
            if (commit == false)
            {
                throw new NotSupportedException();
            }

            if (state == BinaryState.High)
            {
                _bridge.Send(_onCode);
            }
            else if (state == BinaryState.Low)
            {
                _bridge.Send(_offCode);
            }
            else
            {
                throw new NotSupportedException();
            }

            _state = state;
        }

        public BinaryState Read()
        {
            return _state;
        }

        public IBinaryOutput WithInvertedState()
        {
            throw new NotSupportedException();
        }
    }
}
