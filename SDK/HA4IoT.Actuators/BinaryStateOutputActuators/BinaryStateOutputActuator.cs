using System;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Actuators
{
    public abstract class BinaryStateOutputActuator<TSettings> : BinaryStateOutputActuatorBase<TSettings> where TSettings : ActuatorSettings
    {
        private readonly IBinaryStateEndpoint _endpoint;

        private BinaryActuatorState _state = BinaryActuatorState.Off;

        protected BinaryStateOutputActuator(ActuatorId id, IBinaryStateEndpoint endpoint, IApiController apiController, ILogger logger) 
            : base(id, apiController, logger)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            _endpoint = endpoint;
            _endpoint.TurnOff(new ForceUpdateStateParameter());
        }

        protected override void SetStateInternal(BinaryActuatorState state, params IHardwareParameter[] parameters)
        {
            bool forceUpdate = parameters.Any(p => p is ForceUpdateStateParameter);
            if (!forceUpdate && state == _state)
            {
                return;
            }

            if (state == BinaryActuatorState.On)
            {
                _endpoint.TurnOn(parameters);
            }
            else
            {
                _endpoint.TurnOff(parameters);
            }

            bool commit = !parameters.Any(p => p is IsPartOfPartialUpdateParameter);
            if (!commit)
            {
                return;
            }

            BinaryActuatorState oldState = _state;
            _state = state;
            
            OnStateChanged(oldState, _state);
            Logger.Info($"{Id}:{oldState}->{state}");
        }

        protected override BinaryActuatorState GetStateInternal()
        {
            return _state;
        }
    }
}