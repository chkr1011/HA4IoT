using System;

namespace CK.HomeAutomation.Hardware.RemoteSwitch.Codes
{
    public class IntertechnoCodeSequenceProvider
    {
        public LPD433MhzCodeSequence GetSequence(IntertechnoSystemCode systemCode, IntertechnoUnitCode unitCode, RemoteSwitchCommand command)
        {
            switch (systemCode)
            {
                case IntertechnoSystemCode.A:
                {
                    return GetSequenceA(unitCode, command);
                }

                case IntertechnoSystemCode.B:
                {
                    return GetSequenceB(unitCode, command);
                }

                case IntertechnoSystemCode.C:
                {
                    return GetSequenceC(unitCode, command);
                }

                case IntertechnoSystemCode.D:
                {
                    return GetSequenceD(unitCode, command);
                }
            }

            throw new NotSupportedException();
        }

        private LPD433MhzCodeSequence GetSequenceA(IntertechnoUnitCode unitCode, RemoteSwitchCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                {
                    return command == RemoteSwitchCommand.TurnOn
                        ? new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(21, 24))
                        : new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(20, 24));
                }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(16405, 24))
                            : new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(16404, 24));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(4117, 24))
                            : new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(4116, 24));
                    }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        private LPD433MhzCodeSequence GetSequenceB(IntertechnoUnitCode unitCode, RemoteSwitchCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(4194325, 24))
                            : new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(4194324, 24));
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(4210709, 24))
                            : new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(4210708, 24));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(4198421, 24))
                            : new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(4198420, 24));
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private LPD433MhzCodeSequence GetSequenceC(IntertechnoUnitCode unitCode, RemoteSwitchCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(1048597, 24))
                            : new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(1048596, 24));
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(1064981, 24))
                            : new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(1064980, 24));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(1052693, 24))
                            : new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(1052692, 24));
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private LPD433MhzCodeSequence GetSequenceD(IntertechnoUnitCode unitCode, RemoteSwitchCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(5242901, 24))
                            : new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(5242900, 24));
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(5259285, 24))
                            : new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(5259284, 24));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(5246997, 24))
                            : new LPD433MhzCodeSequence().WithCode(new LPD433MhzCode(5246996, 24));
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }
    }
}
