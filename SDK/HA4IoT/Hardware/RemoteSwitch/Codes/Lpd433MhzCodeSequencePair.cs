using System;

namespace HA4IoT.Hardware.RemoteSwitch.Codes
{
    public class Lpd433MhzCodeSequencePair
    {
        public Lpd433MhzCodeSequencePair(Lpd433MhzCodeSequence onSequence, Lpd433MhzCodeSequence offSequence)
        {
            if (onSequence == null) throw new ArgumentNullException(nameof(onSequence));
            if (offSequence == null) throw new ArgumentNullException(nameof(offSequence));

            OnSequence = onSequence;
            OffSequence = offSequence;
        }

        public Lpd433MhzCodeSequence OnSequence { get; }

        public Lpd433MhzCodeSequence OffSequence { get; }
    }
}
