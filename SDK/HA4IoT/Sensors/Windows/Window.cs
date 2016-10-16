using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Triggers;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Sensors.Windows
{
    public class Window : SensorBase, IWindow
    {
        private readonly Trigger _openedTrigger = new Trigger();
        private readonly Trigger _closedTrigger = new Trigger();

        public Window(ComponentId id, ISettingsService settingsService)
            : base(id)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            settingsService.CreateSettingsMonitor<ComponentSettings>(Id, s => Settings = s);
        }

        public IComponentSettings Settings { get; private set; }

        public IList<ICasement> Casements { get; } = new List<ICasement>();

        public ITrigger GetOpenedTrigger()
        {
            return _openedTrigger;
        }

        public ITrigger GetClosedTrigger()
        {
            return _closedTrigger;
        }

        public Window WithCasement(Casement casement)
        {
            Casements.Add(casement);
            casement.StateChanged += (s, e) => OnCasementStateChanged();

            return this;
        }

        public Window WithLeftCasement(IBinaryInput fullOpenReedSwitch, IBinaryInput tiltReedSwitch = null)
        {
            return WithCasement(new Casement(Casement.LeftCasementId, fullOpenReedSwitch, tiltReedSwitch));
        }

        public Window WithCenterCasement(IBinaryInput fullOpenReedSwitch, IBinaryInput tiltReedSwitch = null)
        {
            return WithCasement(new Casement(Casement.CenterCasementId, fullOpenReedSwitch, tiltReedSwitch));
        }

        public Window WithRightCasement(IBinaryInput fullOpenReedSwitch, IBinaryInput tiltReedSwitch = null)
        {
            return WithCasement(new Casement(Casement.RightCasementId, fullOpenReedSwitch, tiltReedSwitch));
        }

        public override JToken ExportStatus()
        {
            var status = base.ExportStatus();

            var state = new JObject();
            foreach (var casement in Casements)
            {
                state[casement.Id] = casement.GetState().JToken;
            }

            status["State"] = state;

            return status;
        }

        public override IList<ComponentState> GetSupportedStates()
        {
            return new List<ComponentState> { CasementStateId.Closed, CasementStateId.Tilt, CasementStateId.Open };
        }

        public override JToken ExportConfiguration()
        {
            var configuration = base.ExportConfiguration();

            var casements = new JArray();
            foreach (var casement in Casements)
            {
                casements.Add(casement.Id);
            }

            configuration["Casements"] = casements;

            return configuration;
        }

        private void OnCasementStateChanged()
        {
            var oldState = GetState();
            var newState = GetStateInternal();

            if (oldState.Equals(newState))
            {
                return;
            }

            if (!Settings.IsEnabled)
            {
                return;
            }

            SetState(newState);
        }

        private ComponentState GetStateInternal()
        {
            if (Casements.Any(c => c.GetState().Equals(CasementStateId.Open)))
            {
                return CasementStateId.Open;
            }

            if (Casements.Any(c => c.GetState().Equals(CasementStateId.Tilt)))
            {
                return CasementStateId.Tilt;
            }

            return CasementStateId.Closed;
        }
    }
}
