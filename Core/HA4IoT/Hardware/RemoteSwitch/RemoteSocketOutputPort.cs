using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.RemoteSwitch.Codes;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class RemoteSocketOutputPort : IBinaryOutput
    {
        private readonly object _syncRoot = new object();
        private readonly Lpd433MhzCodePair _codePair;
        private readonly RemoteSocketService _remoteSocketService;
        private BinaryState _state;

        public RemoteSocketOutputPort(Lpd433MhzCodePair codePair, RemoteSocketService remoteSocketService)
        {
            _codePair = codePair ?? throw new ArgumentNullException(nameof(codePair));
            _remoteSocketService = remoteSocketService ?? throw new ArgumentNullException(nameof(remoteSocketService));
        }

        public void Write(BinaryState state, WriteBinaryStateMode mode = WriteBinaryStateMode.Commit)
        {
            if (mode != WriteBinaryStateMode.Commit)
            {
                return;
            }

            lock (_syncRoot)
            {
                if (state == BinaryState.High)
                {
                    _remoteSocketService.SendCode(_codePair.OnCode);
                }
                else if (state == BinaryState.Low)
                {
                    _remoteSocketService.SendCode(_codePair.OffCode);
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

        public IBinaryOutput WithInvertedState(bool value = true)
        {
            throw new NotSupportedException();
        }
    }
}
