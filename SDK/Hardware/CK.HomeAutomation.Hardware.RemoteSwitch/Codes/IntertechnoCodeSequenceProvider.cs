using System;

namespace CK.HomeAutomation.Hardware.RemoteSwitch.Codes
{
    public class IntertechnoCodeSequenceProvider
    {
        public LPD433MHzCodeSequence GetSequence(IntertechnoSystemCode systemCode, IntertechnoUnitCode unitCode, RemoteSwitchCommand command)
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

        private LPD433MHzCodeSequence GetSequenceA(IntertechnoUnitCode unitCode, RemoteSwitchCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                {
                    return command == RemoteSwitchCommand.TurnOn
                        ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(21, 24))
                        : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(20, 24));
                }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(16405, 24))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(16404, 24));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4117, 24))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4116, 24));
                    }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        private LPD433MHzCodeSequence GetSequenceB(IntertechnoUnitCode unitCode, RemoteSwitchCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4194325, 24))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4194324, 24));
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4210709, 24))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4210708, 24));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4198421, 24))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4198420, 24));
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private LPD433MHzCodeSequence GetSequenceC(IntertechnoUnitCode unitCode, RemoteSwitchCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(1048597, 24))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(1048596, 24));
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(1064981, 24))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(1064980, 24));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(1052693, 24))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(1052692, 24));
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private LPD433MHzCodeSequence GetSequenceD(IntertechnoUnitCode unitCode, RemoteSwitchCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(5242901, 24))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(5242900, 24));
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(5259285, 24))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(5259284, 24));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSwitchCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(5246997, 24))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(5246996, 24));
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }
    }
}
