using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class Window : ActuatorBase<ActuatorSettings>
    {
        private readonly Trigger _openedTrigger = new Trigger();
        private readonly Trigger _closedTrigger = new Trigger();

        public Window(ActuatorId id, IApiController apiController) 
            : base(id, apiController)
        {
            Settings = new ActuatorSettings(id);
        }

        public IList<Casement> Casements { get; } = new List<Casement>();

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
            casement.StateChanged += ExecuteTriggers;

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
                state.SetNamedValue(casement.Id, casement.GetState().ToJsonValue());
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

        private void ExecuteTriggers(object sender, CasementStateChangedEventArgs e)
        {
            if (Casements.Count(c => c.GetState() != CasementState.Closed) == 1)
            {
                _openedTrigger.Execute();
            }
            else if (Casements.All(c => c.GetState() == CasementState.Closed))
            {
                _closedTrigger.Execute();
            }

            ApiController.NotifyStateChanged(this);
        }
    }
}
