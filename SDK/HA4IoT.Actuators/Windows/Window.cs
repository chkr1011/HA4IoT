using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class Window : ActuatorBase
    {
        private readonly List<Casement> _casements = new List<Casement>(); 

        public Window(ActuatorId id, IHttpRequestController api, ILogger logger) : base(id, api, logger)
        {
        }

        public Window WithCasement(Casement casement)
        {
            _casements.Add(casement);
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

        public override JsonObject GetStatusForApi()
        {
            var status = base.GetStatusForApi();

            var state = new JsonObject();
            foreach (var casement in _casements)
            {
                state.SetNamedValue(casement.Id, JsonValue.CreateStringValue(casement.State.ToString()));
            }

            status.SetNamedValue("state", state);

            return status;
        }
        
        public override JsonObject GetConfigurationForApi()
        {
            JsonObject configuration = base.GetConfigurationForApi();

            JsonArray casements = new JsonArray();
            foreach (var casement in _casements)
            {
                casements.Add(JsonValue.CreateStringValue(casement.Id));
            }

            configuration.SetNamedValue("casements", casements);

            return configuration;
        }
    }
}
