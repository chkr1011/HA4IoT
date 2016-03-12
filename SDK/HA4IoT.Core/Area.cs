using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using System;
using System.Collections.Generic;
using Windows.Data.Json;
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

            Settings = new AreaSettings(id, controller.Logger);
        }

        public AreaId Id { get; }

        public IAreaSettings Settings { get; }

        public IController Controller { get; }

        public void AddActuator(IActuator actuator)
        {
            _actuators.AddUnique(actuator.Id, actuator);
            Controller.AddActuator(actuator);
        }

        public TActuator GetActuator<TActuator>() where TActuator : IActuator
        {
            return _actuators.Get<TActuator>();
        }

        public IList<TActuator> GetActuators<TActuator>() where TActuator : IActuator
        {
            return _actuators.GetAll<TActuator>();
        }

        public IList<IActuator> GetActuators()
        {
            return _actuators.GetAll();
        }

        public TActuator GetActuator<TActuator>(ActuatorId id) where TActuator : IActuator
        {
            return _actuators.Get<TActuator>(id);
        }

        public void AddAutomation(IAutomation automation)
        {
            _automations.AddUnique(automation.Id, automation);
            Controller.AddAutomation(automation);
        }

        public IList<TAutomation> GetAutomations<TAutomation>() where TAutomation : IAutomation
        {
            return _automations.GetAll<TAutomation>();
        }

        public TAutomation GetAutomation<TAutomation>(AutomationId id) where TAutomation : IAutomation
        {
            return _automations.Get<TAutomation>(id);
        }

        public IList<IAutomation> GetAutomations()
        {
            return _automations.GetAll();
        }

        public void LoadSettings()
        {
            Settings?.Load();
        }

        public JsonObject ExportConfigurationToJsonObject()
        {
            return Settings.ExportToJsonObject();
        }

        public void ExposeToApi()
        {
            new AreaSettingsApiDispatcher(Settings, Controller.ApiController).ExposeToApi();
        }
    }
}