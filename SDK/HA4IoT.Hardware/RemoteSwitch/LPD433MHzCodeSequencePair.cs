using System;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class LPD433MHzCodeSequencePair
    {
        public LPD433MHzCodeSequencePair(LPD433MHzCodeSequence onSequence, LPD433MHzCodeSequence offSequence)
        {
            if (onSequence == null) throw new ArgumentNullException(nameof(onSequence));
            if (offSequence == null) throw new ArgumentNullException(nameof(offSequence));

            OnSequence = onSequence;
            OffSequence = offSequence;
        }

        public LPD433MHzCodeSequence OnSequence { get; }

        public LPD433MHzCodeSequence OffSequence { get; }
    }
}
