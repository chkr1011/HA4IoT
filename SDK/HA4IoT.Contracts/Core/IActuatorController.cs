using System.Collections.Generic;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Contracts.Core
{
    public interface IActuatorController
    {
        void AddActuator(IActuator actuator);

        TActuator GetActuator<TActuator>(ActuatorId id) where TActuator : IActuator;

        TActuator GetActuator<TActuator>() where TActuator : IActuator;

        IList<TActuator> GetActuators<TActuator>() where TActuator : IActuator;

        IList<IActuator> GetActuators();
    }
}
