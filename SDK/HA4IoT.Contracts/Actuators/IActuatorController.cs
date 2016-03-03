using System.Collections.Generic;

namespace HA4IoT.Contracts.Actuators
{
    public interface IActuatorController
    {
        void AddActuator(IActuator actuator);

        TActuator Actuator<TActuator>(ActuatorId id) where TActuator : IActuator;

        TActuator Actuator<TActuator>() where TActuator : IActuator;

        IList<TActuator> Actuators<TActuator>() where TActuator : IActuator;

        IList<IActuator> Actuators();
    }
}
