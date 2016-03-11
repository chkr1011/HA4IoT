using System;
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
            _endpoint.TurnOff();
        }

        protected override void SetStateInternal(BinaryActuatorState state, params IParameter[] parameters)
        {
            if (state == _state)
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