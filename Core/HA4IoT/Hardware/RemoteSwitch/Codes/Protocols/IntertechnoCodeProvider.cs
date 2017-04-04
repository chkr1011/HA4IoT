using System;

namespace HA4IoT.Hardware.RemoteSwitch.Codes.Protocols
{
    public class IntertechnoCodeProvider
    {
        private const byte LENGTH = 24;
        private const byte REPEATS = 3;

        public Lpd433MhzCodePair GetCodePair(IntertechnoSystemCode systemCode, IntertechnoUnitCode unitCode)
        {
            return new Lpd433MhzCodePair(
                GetCode(systemCode, unitCode, RemoteSocketCommand.TurnOn),
                GetCode(systemCode, unitCode, RemoteSocketCommand.TurnOff));
        }

        public Lpd433MhzCode GetCode(IntertechnoSystemCode systemCode, IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (systemCode)
            {
                case IntertechnoSystemCode.A:
                {
                    return GetCodeA(unitCode, command);
                }

                case IntertechnoSystemCode.B:
                {
                    return GetCodeB(unitCode, command);
                }

                case IntertechnoSystemCode.C:
                {
                    return GetCodeC(unitCode, command);
                }

                case IntertechnoSystemCode.D:
                {
                    return GetCodeD(unitCode, command);
                }
            }

            throw new NotSupportedException();
        }

        private Lpd433MhzCode GetCodeA(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                {
                    return command == RemoteSocketCommand.TurnOn
                        ? new Lpd433MhzCode(21U, LENGTH, REPEATS)
                        : new Lpd433MhzCode(20U, LENGTH, REPEATS);
                }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(16405U, LENGTH, REPEATS)
                            : new Lpd433MhzCode(16404U, LENGTH, REPEATS);
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(4117U, LENGTH, REPEATS)
                            : new Lpd433MhzCode(4116U, LENGTH, REPEATS);
                    }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        private Lpd433MhzCode GetCodeB(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(4194325U, LENGTH, REPEATS)
                            : new Lpd433MhzCode(4194324U, LENGTH, REPEATS);
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(4210709U, LENGTH, REPEATS)
                            : new Lpd433MhzCode(4210708U, LENGTH, REPEATS);
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(4198421U, LENGTH, REPEATS)
                            : new Lpd433MhzCode(4198420U, LENGTH, REPEATS);
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private Lpd433MhzCode GetCodeC(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(1048597U, LENGTH, REPEATS)
                            : new Lpd433MhzCode(1048596U, LENGTH, REPEATS);
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(1064981U, LENGTH, REPEATS)
                            : new Lpd433MhzCode(1064980U, LENGTH, REPEATS);
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(1052693U, LENGTH, REPEATS)
                            : new Lpd433MhzCode(1052692U, LENGTH, REPEATS);
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private Lpd433MhzCode GetCodeD(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(5242901U, LENGTH, REPEATS)
                            : new Lpd433MhzCode(5242900U, LENGTH, REPEATS);
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(5259285U, LENGTH, REPEATS)
                            : new Lpd433MhzCode(5259284U, LENGTH, REPEATS);
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(5246997U, LENGTH, REPEATS)
                            : new Lpd433MhzCode(5246996U, LENGTH, REPEATS);
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }
    }
}
