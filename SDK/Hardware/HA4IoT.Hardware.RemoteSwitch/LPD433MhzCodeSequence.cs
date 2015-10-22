using System;
using System.Collections.Generic;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class LPD433MHzCodeSequence
    {
        public IList<LPD433MHzCode> Codes { get; } = new List<LPD433MHzCode>();

        public LPD433MHzCodeSequence WithCode(LPD433MHzCode code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));

            Codes.Add(code);
            return this;
        }
    }
}
