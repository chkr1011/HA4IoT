using System.Collections.Generic;

namespace HA4IoT.Contracts.Actuators
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
