using System;
using CK.HomeAutomation.Core.Timer;

namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class RemoteSwitchOutputPort : IBinaryOutput
    {
        private readonly RemoteSwitchCode _offCode;
        private readonly RemoteSwitchCode _onCode;
        private readonly Wireless433MhzSignalSender _sender;
        private readonly object _syncRoot = new object();
        private BinaryState _state;

        public RemoteSwitchOutputPort(int id, RemoteSwitchCode onCode, RemoteSwitchCode offCode, Wireless433MhzSignalSender sender, IHomeAutomationTimer timer)
        {
            if (onCode == null) throw new ArgumentNullException(nameof(onCode));
            if (offCode == null) throw new ArgumentNullException(nameof(offCode));
            if (sender == null) throw new ArgumentNullException(nameof(sender));

            _onCode = onCode;
            _offCode = offCode;
            _sender = sender;

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

            lock (_syncRoot)
            {
                if (state == BinaryState.High)
                {
                    _sender.Send(_onCode);
                }
                else if (state == BinaryState.Low)
                {
                    _sender.Send(_offCode);
                }
                else
                {
                    throw new NotSupportedException();
                }

                _state = state;
            }
        }

        public BinaryState Read()
        {
            lock (_syncRoot)
            {
                return _state;
            }
        }

        public IBinaryOutput WithInvertedState()
        {
            throw new NotSupportedException();
        }
    }
}
