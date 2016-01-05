using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Core
{
    public class Room : IRoom
    {
        private readonly Dictionary<ActuatorId, IActuator> _actuators = new Dictionary<ActuatorId, IActuator>();
        
        public Room(RoomId id, IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            Id = id;
            Controller = controller;
        }

        public RoomId Id { get; }

        public IController Controller { get; }

        public IReadOnlyDictionary<ActuatorId, IActuator> Actuators => _actuators;

        public void AddActuator(IActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            if (Controller.Actuators.ContainsKey(actuator.Id))
            {
                throw new InvalidOperationException("Actuator with ID '" + actuator.Id + "' aready registered.");
            }

            Controller.Actuators.Add(actuator.Id, actuator);
            _actuators.Add(actuator.Id, actuator);
        }

        public TActuator Actuator<TActuator>(ActuatorId id) where TActuator : IActuator
        {
            IActuator actuator;
            if (!_actuators.TryGetValue(id, out actuator))
            {
                throw new InvalidOperationException("Actuator with ID '" + id + "' is not registered.");
            }

            return (TActuator)actuator;
        }
    }
}