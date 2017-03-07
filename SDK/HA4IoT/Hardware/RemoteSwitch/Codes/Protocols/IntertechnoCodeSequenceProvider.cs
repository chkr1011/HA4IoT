using System;

namespace HA4IoT.Hardware.RemoteSwitch.Codes.Protocols
{
    public class IntertechnoCodeSequenceProvider
    {
        private const byte LENGTH = 24;
        private const byte REPEATS = 3;

        public Lpd433MhzCodeSequencePair GetSequencePair(IntertechnoSystemCode systemCode, IntertechnoUnitCode unitCode)
        {
            return new Lpd433MhzCodeSequencePair(
                GetSequence(systemCode, unitCode, RemoteSocketCommand.TurnOn),
                GetSequence(systemCode, unitCode, RemoteSocketCommand.TurnOff));
        }

        public Lpd433MhzCodeSequence GetSequence(IntertechnoSystemCode systemCode, IntertechnoUnitCode unitCode, RemoteSocketCommand command)
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

        private Lpd433MhzCodeSequence GetSequenceA(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                {
                    return command == RemoteSocketCommand.TurnOn
                        ? new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(21U, LENGTH, REPEATS))
                        : new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(20U, LENGTH, REPEATS));
                }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(16405U, LENGTH, REPEATS))
                            : new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(16404U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(4117U, LENGTH, REPEATS))
                            : new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(4116U, LENGTH, REPEATS));
                    }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        private Lpd433MhzCodeSequence GetSequenceB(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(4194325U, LENGTH, REPEATS))
                            : new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(4194324U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(4210709U, LENGTH, REPEATS))
                            : new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(4210708U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(4198421U, LENGTH, REPEATS))
                            : new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(4198420U, LENGTH, REPEATS));
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private Lpd433MhzCodeSequence GetSequenceC(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(1048597U, LENGTH, REPEATS))
                            : new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(1048596U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(1064981U, LENGTH, REPEATS))
                            : new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(1064980U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(1052693U, LENGTH, REPEATS))
                            : new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(1052692U, LENGTH, REPEATS));
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private Lpd433MhzCodeSequence GetSequenceD(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(5242901U, LENGTH, REPEATS))
                            : new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(5242900U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(5259285U, LENGTH, REPEATS))
                            : new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(5259284U, LENGTH, REPEATS));
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(5246997U, LENGTH, REPEATS))
                            : new Lpd433MhzCodeSequence().WithCode(new Lpd433MhzCode(5246996U, LENGTH, REPEATS));
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }
    }
}
