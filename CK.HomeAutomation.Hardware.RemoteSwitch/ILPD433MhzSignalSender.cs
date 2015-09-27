namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public interface ILPD433MhzSignalSender
    {
        void Send(LPD433MhzCodeSequence codeSequence);
    }
}
