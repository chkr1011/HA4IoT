using System.Collections.Generic;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Configuration
{
    public interface IRoom
    {
        RoomId Id { get; }

        IController Controller { get; }

        void AddActuator(IActuator actuator);

        IList<IActuator> GetActuators();

        TActuator Actuator<TActuator>(ActuatorId id) where TActuator : IActuator;
    }
}
