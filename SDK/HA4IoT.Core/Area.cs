using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using System;
using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Core.Settings;

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

            Settings = new SettingsContainer(StoragePath.WithFilename("Areas", id.Value, "Settings.json"));
        }

        public AreaId Id { get; }

        public ISettingsContainer Settings { get; }

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

        public JsonObject ExportConfigurationToJsonObject()
        {
            return Settings.ExportToJsonObject();
        }

        public void ExposeToApi(IApiController apiController)
        {
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            new AreaSettingsApiDispatcher(this, apiController).ExposeToApi();
        }
    }
}