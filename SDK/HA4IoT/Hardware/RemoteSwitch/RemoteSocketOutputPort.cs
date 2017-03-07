using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.RemoteSwitch.Codes;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class RemoteSocketOutputPort : IBinaryOutput
    {
        private readonly object _syncRoot = new object();
        private readonly Lpd433MhzCodeSequencePair _codeSequencePair;
        private readonly RemoteSocketService _remoteSocketService;
        private BinaryState _state;

        public RemoteSocketOutputPort(Lpd433MhzCodeSequencePair codeSequencePair, RemoteSocketService remoteSocketService)
        {
            if (codeSequencePair == null) throw new ArgumentNullException(nameof(codeSequencePair));
            if (remoteSocketService == null) throw new ArgumentNullException(nameof(remoteSocketService));

            _codeSequencePair = codeSequencePair;
            _remoteSocketService = remoteSocketService;
        }

        public void Write(BinaryState state, bool commit = true)
        {
            if (commit == false)
            {
                return;
            }

            lock (_syncRoot)
            {
                if (state == BinaryState.High)
                {
                    _remoteSocketService.SendCodeSequence(_codeSequencePair.OnSequence);
                }
                else if (state == BinaryState.Low)
                {
                    _remoteSocketService.SendCodeSequence(_codeSequencePair.OffSequence);
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
