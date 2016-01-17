using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using System;
using System.Collections.Generic;

namespace HA4IoT.Core
{
    public class Room : IRoom
    {
        private readonly ActuatorCollection _actuators = new ActuatorCollection();

        public Room(RoomId id, IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            Id = id;
            Controller = controller;
        }

        public RoomId Id { get; }

        public IController Controller { get; }

        public void AddActuator(IActuator actuator)
        {
            _actuators.Add(actuator);
            Controller.AddActuator(actuator);
        }

        public IList<IActuator> Actuators()
        {
            return _actuators.GetAll();
        }

        public TActuator Actuator<TActuator>(ActuatorId id) where TActuator : IActuator
        {
            return (TActuator)_actuators.Get(id);
        }
    }
}