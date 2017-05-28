using System;
using HA4IoT.Contracts.Hardware.RemoteSockets.Codes;
using HA4IoT.Contracts.Hardware.RemoteSockets.Protocols;

namespace HA4IoT.Hardware.RemoteSockets.Protocols
{
    public class IntertechnoCodeProvider
    {
        private const byte Length = 24;
        private const byte Protocol = 2;
        private const byte Repeats = 3;

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
                            ? new Lpd433MhzCode(21U, Length, Protocol, Repeats)
                            : new Lpd433MhzCode(20U, Length, Protocol, Repeats);
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(16405U, Length, Protocol, Repeats)
                            : new Lpd433MhzCode(16404U, Length, Protocol, Repeats);
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(4117U, Length, Protocol, Repeats)
                            : new Lpd433MhzCode(4116U, Length, Protocol, Repeats);
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
                            ? new Lpd433MhzCode(4194325U, Length, Protocol, Repeats)
                            : new Lpd433MhzCode(4194324U, Length, Protocol, Repeats);
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(4210709U, Length, Protocol, Repeats)
                            : new Lpd433MhzCode(4210708U, Length, Protocol, Repeats);
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(4198421U, Length, Protocol, Repeats)
                            : new Lpd433MhzCode(4198420U, Length, Protocol, Repeats);
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
                            ? new Lpd433MhzCode(1048597U, Length, Protocol, Repeats)
                            : new Lpd433MhzCode(1048596U, Length, Protocol, Repeats);
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(1064981U, Length, Protocol, Repeats)
                            : new Lpd433MhzCode(1064980U, Length, Protocol, Repeats);
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(1052693U, Length, Protocol, Repeats)
                            : new Lpd433MhzCode(1052692U, Length, Protocol, Repeats);
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
                            ? new Lpd433MhzCode(5242901U, Length, Protocol, Repeats)
                            : new Lpd433MhzCode(5242900U, Length, Protocol, Repeats);
                    }

                case IntertechnoUnitCode.Unit2:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(5259285U, Length, Protocol, Repeats)
                            : new Lpd433MhzCode(5259284U, Length, Protocol, Repeats);
                    }

                case IntertechnoUnitCode.Unit3:
                    {
                        return command == RemoteSocketCommand.TurnOn
                            ? new Lpd433MhzCode(5246997U, Length, Protocol, Repeats)
                            : new Lpd433MhzCode(5246996U, Length, Protocol, Repeats);
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }
    }
}
