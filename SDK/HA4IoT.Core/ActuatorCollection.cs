using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Core
{
    public class ActuatorCollection
    {
        private readonly Dictionary<ActuatorId, IActuator> _actuators = new Dictionary<ActuatorId, IActuator>();

        public void AddUnique(IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _actuators[actuator.Id] = actuator;
        }

        public void Add(IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _actuators[actuator.Id] = actuator;
        }

        public TActuator Get<TActuator>(ActuatorId id) where TActuator : IActuator
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            IActuator actuator;
            if (!_actuators.TryGetValue(id, out actuator))
            {
                throw new InvalidOperationException("Actuator with ID '" + id + "' is not registered.");
            }

            return (TActuator) actuator;
        }
        public IList<IActuator> GetAll()
        {
            return _actuators.Values.ToList();
        }
    }
}
