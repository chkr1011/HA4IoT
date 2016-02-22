using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Automations;

namespace HA4IoT.Core
{
    public class Area : IArea
    {
        private readonly ActuatorCollection _actuators = new ActuatorCollection();
        private readonly AutomationCollection _automations = new AutomationCollection();

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
            _actuators.AddUnique(actuator.Id, actuator);
            Controller.AddActuator(actuator);
        }

        public TActuator Actuator<TActuator>() where TActuator : IActuator
        {
            return _actuators.Get<TActuator>();
        }

        public IList<TActuator> Actuators<TActuator>() where TActuator : IActuator
        {
            return _actuators.GetAll<TActuator>();
        }

        public IList<IActuator> Actuators()
        {
            return _actuators.GetAll();
        }

        public TActuator Actuator<TActuator>(ActuatorId id) where TActuator : IActuator
        {
            return _actuators.Get<TActuator>(id);
        }

        public void AddAutomation(IAutomation automation)
        {
            _automations.AddUnique(automation.Id, automation);
            Controller.AddAutomation(automation);
        }

        public IList<TAutomation> Automations<TAutomation>() where TAutomation : IAutomation
        {
            return _automations.GetAll<TAutomation>();
        }

        public TAutomation Automation<TAutomation>(AutomationId id) where TAutomation : IAutomation
        {
            return _automations.Get<TAutomation>(id);
        }

        public IList<IAutomation> Automations()
        {
            return _automations.GetAll();
        }
    }
}