namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class RemoteSwitchCode
    {
        public RemoteSwitchCode(ulong code, int length)
        {
            Code = code;
            Length = length;
        }

        public ulong Code { get; }

        public int Length { get; }
    }
}
