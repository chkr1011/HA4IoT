using System;
using HA4IoT.Contracts.Hardware.RemoteSockets.Codes;
using HA4IoT.Contracts.Hardware.RemoteSockets.Protocols;
using HA4IoT.Hardware.RemoteSockets;

namespace HA4IoT.Hardware.Drivers.RemoteSockets
{
    public static class IntertechnoCodeProvider
    {
        private const byte Length = 24;
        private const byte Protocol = 2;
        private const byte Repeats = 3;

        public static Lpd433MhzCodePair GetCodePair(IntertechnoSystemCode systemCode, IntertechnoUnitCode unitCode)
        {
            return new Lpd433MhzCodePair(
                GetCode(systemCode, unitCode, RemoteSocketCommand.TurnOn),
                GetCode(systemCode, unitCode, RemoteSocketCommand.TurnOff));
        }

        public static Lpd433MhzCode GetCode(IntertechnoSystemCode systemCode, IntertechnoUnitCode unitCode, RemoteSocketCommand command)
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

        private static Lpd433MhzCode GetCodeA(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode { Value = 21U, Length = Length, Protocol = Protocol, Repeats = Repeats }
                            : new Lpd433MhzCode { Value = 20U, Length = Length, Protocol = Protocol, Repeats = Repeats };
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode { Value = 16405U, Length = Length, Protocol = Protocol, Repeats = Repeats }
                            : new Lpd433MhzCode { Value = 16404U, Length = Length, Protocol = Protocol, Repeats = Repeats };
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode { Value = 4117U, Length = Length, Protocol = Protocol, Repeats = Repeats }
                            : new Lpd433MhzCode { Value = 4116U, Length = Length, Protocol = Protocol, Repeats = Repeats };
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private static Lpd433MhzCode GetCodeB(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode { Value = 4194325U, Length = Length, Protocol = Protocol, Repeats = Repeats }
                            : new Lpd433MhzCode { Value = 4194324U, Length = Length, Protocol = Protocol, Repeats = Repeats };
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode { Value = 4210709U, Length = Length, Protocol = Protocol, Repeats = Repeats }
                            : new Lpd433MhzCode { Value = 4210708U, Length = Length, Protocol = Protocol, Repeats = Repeats };
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode { Value = 4198421U, Length = Length, Protocol = Protocol, Repeats = Repeats }
                            : new Lpd433MhzCode { Value = 4198420U, Length = Length, Protocol = Protocol, Repeats = Repeats };
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private static Lpd433MhzCode GetCodeC(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSocketCommand.TurnOn
                                ? new Lpd433MhzCode { Value = 1048597U, Length = Length, Protocol = Protocol, Repeats = Repeats }
                                : new Lpd433MhzCode { Value = 1048596U, Length = Length, Protocol = Protocol, Repeats = Repeats };
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode { Value = 1064981U, Length = Length, Protocol = Protocol, Repeats = Repeats }
                            : new Lpd433MhzCode { Value = 1064980U, Length = Length, Protocol = Protocol, Repeats = Repeats };
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode { Value = 1052693U, Length = Length, Protocol = Protocol, Repeats = Repeats }
                            : new Lpd433MhzCode { Value = 1052692U, Length = Length, Protocol = Protocol, Repeats = Repeats };
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private static Lpd433MhzCode GetCodeD(IntertechnoUnitCode unitCode, RemoteSocketCommand command)
        {
            switch (unitCode)
            {
                case IntertechnoUnitCode.Unit1:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode { Value = 5242901U, Length = Length, Protocol = Protocol, Repeats = Repeats }
                            : new Lpd433MhzCode { Value = 5242900U, Length = Length, Protocol = Protocol, Repeats = Repeats };
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode { Value = 5259285U, Length = Length, Protocol = Protocol, Repeats = Repeats }
                            : new Lpd433MhzCode { Value = 5259284U, Length = Length, Protocol = Protocol, Repeats = Repeats };
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode { Value = 5246997U, Length = Length, Protocol = Protocol, Repeats = Repeats }
                            : new Lpd433MhzCode { Value = 5246996U, Length = Length, Protocol = Protocol, Repeats = Repeats };
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }
    }
}
