namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class LPD433MhzCode
    {
        public LPD433MhzCode(ulong code, int length)
        {
            Code = code;
            Length = length;
        }

        public ulong Code { get; }

        public int Length { get; }
    }
}
