using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class RemoteSocketOutputPort : IBinaryOutput
    {
        private readonly LPD433MHzCodeSequencePair _codeSequencePair;
        private readonly LPD433MHzSignalSender _sender;
        private readonly object _syncRoot = new object();
        private BinaryState _state;

        public RemoteSocketOutputPort(LPD433MHzCodeSequencePair codeSequencePair, LPD433MHzSignalSender sender)
        {
            if (codeSequencePair == null) throw new ArgumentNullException(nameof(codeSequencePair));
            if (sender == null) throw new ArgumentNullException(nameof(sender));

            _codeSequencePair = codeSequencePair;
            _sender = sender;
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
                    _sender.Send(_codeSequencePair.OnSequence);
                }
                else if (state == BinaryState.Low)
                {
                    _sender.Send(_codeSequencePair.OffSequence);
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
