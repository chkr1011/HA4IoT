using System;

namespace HA4IoT.Hardware.RemoteSwitch.Codes
{
    public class IntertechnoCodeSequenceProvider
    {
        private const byte LENGTH = 24;
        private const byte REPEATS = 3;

        public LPD433MHzCodeSequencePair GetSequencePair(IntertechnoSystemCode systemCode, IntertechnoUnitCode unitCode)
        {
            return new LPD433MHzCodeSequencePair(
                GetSequence(systemCode, unitCode, RemoteSocketCommand.TurnOn),
                GetSequence(systemCode, unitCode, RemoteSocketCommand.TurnOff));
        }

        public LPD433MHzCodeSequence GetSequence(IntertechnoSystemCode systemCode, IntertechnoUnitCode unitCode, RemoteSocketCommand command)
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

        private LPD433MHzCodeSequence GetSequenceA(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                {
                    return command == RemoteSocketCommand.TurnOn
                        ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(21U, LENGTH, REPEATS))
                        : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(20U, LENGTH, REPEATS));
                }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(16405U, LENGTH, REPEATS))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(16404U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4117U, LENGTH, REPEATS))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4116U, LENGTH, REPEATS));
                    }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        private LPD433MHzCodeSequence GetSequenceB(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4194325U, LENGTH, REPEATS))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4194324U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4210709U, LENGTH, REPEATS))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4210708U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4198421U, LENGTH, REPEATS))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(4198420U, LENGTH, REPEATS));
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private LPD433MHzCodeSequence GetSequenceC(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(1048597U, LENGTH, REPEATS))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(1048596U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(1064981U, LENGTH, REPEATS))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(1064980U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(1052693U, LENGTH, REPEATS))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(1052692U, LENGTH, REPEATS));
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private LPD433MHzCodeSequence GetSequenceD(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(5242901U, LENGTH, REPEATS))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(5242900U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(5259285U, LENGTH, REPEATS))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(5259284U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(5246997U, LENGTH, REPEATS))
                            : new LPD433MHzCodeSequence().WithCode(new LPD433MHzCode(5246996U, LENGTH, REPEATS));
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }
    }
}
