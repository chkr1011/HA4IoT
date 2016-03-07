using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;

namespace HA4IoT.Actuators
{
    public class Lamp : BinaryStateOutputActuator<ActuatorSettings>, ILamp
    {
        private readonly IBinaryStateEndpoint _endpoint;

        private BinaryActuatorState _state = BinaryActuatorState.Off;

        public Lamp(ActuatorId id, IBinaryStateEndpoint endpoint, IHttpRequestController httpApiController, ILogger logger)
            : base(id, endpoint, httpApiController, logger)
        {
            _endpoint = endpoint;

            Settings = new ActuatorSettings(id, logger);
            _endpoint.TurnOff();
        }

        public event EventHandler<BinaryActuatorStateChangedEventArgs> StateChanged;

        public BinaryActuatorState GetState()
        {
            return _state;
        }

        public void SetState(BinaryActuatorState state, params IParameter[] parameters)
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

            StateChanged?.Invoke(this, new BinaryActuatorStateChangedEventArgs(oldState, _state));
        }

        public void TurnOff(params IParameter[] parameters)
        {
            SetState(BinaryActuatorState.Off, parameters);
        }
    }
}