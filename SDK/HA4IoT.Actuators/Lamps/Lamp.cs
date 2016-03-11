using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Actuators
{
    public class Lamp : BinaryStateOutputActuator<ActuatorSettings>, ILamp
    {
        public Lamp(ActuatorId id, IBinaryStateEndpoint endpoint, IApiController apiController, ILogger logger)
            : base(id, endpoint, apiController, logger)
        {
            Settings = new ActuatorSettings(id, logger);
        }
    }
}