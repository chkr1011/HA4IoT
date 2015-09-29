using System.Collections.Generic;

namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class LPD433MhzCodeSequence
    {
        public IList<LPD433MhzCode> Codes { get; } = new List<LPD433MhzCode>();

        public LPD433MhzCodeSequence WithCode(LPD433MhzCode code)
        {
            Codes.Add(code);
            return this;
        }
    }
}
