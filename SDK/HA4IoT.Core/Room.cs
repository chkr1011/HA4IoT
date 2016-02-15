using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using System;
using System.Collections.Generic;

namespace HA4IoT.Core
{
    public class Area : IArea
    {
        private readonly ActuatorCollection _actuators = new ActuatorCollection();

        public Area(AreaId id, IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            Id = id;
            Controller = controller;
        }

        public AreaId Id { get; }

        public IController Controller { get; }

        public void AddActuator(IActuator actuator)
        {
            _actuators.AddUnique(actuator);
            Controller.AddActuator(actuator);
        }

        public IList<IActuator> Actuators()
        {
            return _actuators.GetAll();
        }

        public TActuator Actuator<TActuator>(ActuatorId id) where TActuator : IActuator
        {
            return _actuators.Get<TActuator>(id);
        }
    }
}