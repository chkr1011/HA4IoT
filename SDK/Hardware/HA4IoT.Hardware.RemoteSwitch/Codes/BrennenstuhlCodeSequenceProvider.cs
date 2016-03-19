using System;

namespace HA4IoT.Hardware.RemoteSwitch.Codes
{
    public class BrennenstuhlCodeSequenceProvider
    {
        public LPD433MHzCodeSequencePair GetSequencePair(BrennenstuhlSystemCode systemCode, BrennenstuhlUnitCode unitCode)
        {
            return new LPD433MHzCodeSequencePair(
                GetSequence(systemCode, unitCode, RemoteSocketCommand.TurnOn),
                GetSequence(systemCode, unitCode, RemoteSocketCommand.TurnOff));
        }

        public LPD433MHzCodeSequence GetSequence(BrennenstuhlSystemCode systemCode, BrennenstuhlUnitCode unitCode, RemoteSocketCommand command)
        {
            // Examples:
            // System Code = 11111
            //00000000|00000000000|0010101|010001 = 1361 A ON
            //00000000|00000000000|0010101|010100 = 1364 A OFF
            //00000000|00000000000|1000101|010001 = 4433 B ON
            //00000000|00000000000|1000101|010100 = 4436 B OFF
            //00000000|00000000000|1010001|010001 = 5201 C ON
            //00000000|00000000000|1010001|010100 = 5204 C OFF
            //00000000|00000000000|1010100|010001 = 5393 D ON
            //00000000|00000000000|1010100|010100 = 5396 D OFF
            // System Code = 00000
            //00000000|01010101010|0010101|010001 = 5588305 A ON
            //00000000|01010101010|0010101|010100 = 5588308 A OFF
            //00000000|01010101010|1000101|010001 = 5591377 B ON
            //00000000|01010101010|1000101|010100 = 5591380 B OFF
            //00000000|01010101010|1010001|010001 = 5592145 C ON
            //00000000|01010101010|1010001|010100 = 5592148 C OFF
            //00000000|01010101010|1010100|010001 = 5592337 D ON
            //00000000|01010101010|1010100|010100 = 5592340 D OFF
            // System Code = 10101
            //00000000|00010001000|0010101|010001 = 1115473 A ON
            //00000000|00010001000|0010101|010100 = 1115476 A OFF
            //00000000|00010001000|1000101|010001 = 1118545 B ON
            //00000000|00010001000|1000101|010100 = 1118548 B OFF
            //00000000|00010001000|1010001|010001 = 1119313 C ON
            //00000000|00010001000|1010001|010100 = 1119316 C OFF
            //00000000|00010001000|1010100|010001 = 1119505 D ON
            //00000000|00010001000|1010100|010100 = 1119508 D OFF

            uint code = 0U;
            code = SetSystemCode(code, systemCode);
            code = SetUnitCode(code, unitCode);
            code = SetCommand(code, command);

            return new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(code, 24, 3));
        }

        private uint SetSystemCode(uint code, BrennenstuhlSystemCode systemCode)
        {
            // A LOW switch is binary 10 and a HIGH switch is binary 00.
            // The values of the DIP switches are inverted.
            if (!systemCode.HasFlag(BrennenstuhlSystemCode.Switch1))
            {
                code |= 1U << 22;
            }

            if (!systemCode.HasFlag(BrennenstuhlSystemCode.Switch2))
            {
                code |= 1U << 20;
            }

            if (!systemCode.HasFlag(BrennenstuhlSystemCode.Switch3))
            {
                code |= 1U << 18;
            }

            if (!systemCode.HasFlag(BrennenstuhlSystemCode.Switch4))
            {
                code |= 1U << 16;
            }

            if (!systemCode.HasFlag(BrennenstuhlSystemCode.Switch5))
            {
                code |= 1U << 14;
            }

            return code;
        }

        private uint SetUnitCode(uint code, BrennenstuhlUnitCode unitCode)
        {
            uint unitCodeValue;

            switch (unitCode)
            {
                case BrennenstuhlUnitCode.A:
                    {
                        unitCodeValue = 0x15;
                        break;
                    }

                case BrennenstuhlUnitCode.B:
                    {
                        unitCodeValue = 0x45;
                        break;
                    }

                case BrennenstuhlUnitCode.C:
                    {
                        unitCodeValue = 0x51;
                        break;
                    }

                case BrennenstuhlUnitCode.D:
                    {
                        unitCodeValue = 0x54;
                        break;
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }

            code |= unitCodeValue << 6;
            return code;
        }

        private uint SetCommand(uint code, RemoteSocketCommand command)
        {
            switch (command)
            {
                case RemoteSocketCommand.TurnOn:
                    {
                        code |= 0x11;
                        break;
                    }

                case RemoteSocketCommand.TurnOff:
                    {
                        code |= 0x14;
                        break;
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }

            return code;
        }
    }
}
