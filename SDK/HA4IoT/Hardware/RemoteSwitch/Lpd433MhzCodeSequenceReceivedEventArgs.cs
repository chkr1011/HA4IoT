using System;
using HA4IoT.Hardware.RemoteSwitch.Codes;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class Ldp433MhzCodeSequenceReceivedEventArgs : EventArgs
    {
        public Ldp433MhzCodeSequenceReceivedEventArgs(Lpd433MhzCodeSequence codeSequence)
        {
            if (codeSequence == null) throw new ArgumentNullException(nameof(codeSequence));

            CodeSequence = codeSequence;
        }

        public Lpd433MhzCodeSequence CodeSequence { get; }
    }
}
