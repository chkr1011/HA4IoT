using System;

namespace HA4IoT.Hardware.RemoteSwitch.Codes
{
    [Flags]
    public enum BrennenstuhlSystemCode
    {
        AllOff = 0, // Used to affect the state where all DIP switches are off.
        Switch1 = 1,
        Switch2 = 2,
        Switch3 = 4,
        Switch4 = 8,
        Switch5 = 16,
        AllOn = Switch1 | Switch2 | Switch3 | Switch4 | Switch5
    }
}
