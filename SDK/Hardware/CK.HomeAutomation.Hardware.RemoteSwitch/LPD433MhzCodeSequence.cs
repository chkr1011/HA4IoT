using System.Collections.Generic;

namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class LPD433MHzCodeSequence
    {
        public IList<LPD433MHzCode> Codes { get; } = new List<LPD433MHzCode>();

        public LPD433MHzCodeSequence WithCode(LPD433MHzCode code)
        {
            Codes.Add(code);
            return this;
        }
    }
}
