using System.Collections.Generic;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Contracts.Configuration
{
    public interface IArea
    {
        AreaId Id { get; }

        IController Controller { get; }

        void AddActuator(IActuator actuator);

        IList<IActuator> Actuators();

        TActuator Actuator<TActuator>(ActuatorId id) where TActuator : IActuator;
    }
}
