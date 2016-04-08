using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Networking;

namespace HA4IoT.Sensors.Windows
{
    public class Window : SensorBase, IWindow
    {
        private readonly Trigger _openedTrigger = new Trigger();
        private readonly Trigger _closedTrigger = new Trigger();

        public Window(ComponentId id) 
            : base(id)
        {
        }

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

        public Window WithCasement(string id, IBinaryInput fullOpenReedSwitch, IBinaryInput tiltReedSwitch = null)
        {
            return WithCasement(new Casement(id, fullOpenReedSwitch, tiltReedSwitch));
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

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();

            var state = new JsonObject();
            foreach (var casement in Casements)
            {
                state.SetNamedString(casement.Id, casement.GetState().Value);
            }

            status.SetNamedValue("state", state);

            return status;
        }
        
        public override JsonObject ExportConfigurationToJsonObject()
        {
            JsonObject configuration = base.ExportConfigurationToJsonObject();

            JsonArray casements = new JsonArray();
            foreach (var casement in Casements)
            {
                casements.Add(JsonValue.CreateStringValue(casement.Id));
            }

            configuration.SetNamedValue("casements", casements);

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

            if (!GeneralSettingsWrapper.IsEnabled)
            {
                return;
            }

            SetState(newState);
        }

        private StateId GetStateInternal()
        {
            if (Casements.Any(c => c.GetState() == CasementStateId.Open))
            {
                return CasementStateId.Open;
            }

            if (Casements.Any(c => c.GetState() == CasementStateId.Tilt))
            {
                return CasementStateId.Tilt;
            }

            return CasementStateId.Closed;
        }
    }
}
