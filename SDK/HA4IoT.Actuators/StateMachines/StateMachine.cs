using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class StateMachine : ActuatorBase, IStateMachine
    {
        private int _index;
        private bool _turnOffIfStateIsAppliedTwice;

        public StateMachine(ActuatorId id, IHttpRequestController api, ILogger logger)
            : base(id, api, logger)
        {
        }

        public List<StateMachineState> States { get; } = new List<StateMachineState>();

        public bool HasOffState
        {
            get { return States.Any(s => s.Id.Equals(BinaryActuatorState.Off.ToString(), StringComparison.OrdinalIgnoreCase)); }
        }

        public event EventHandler<StateMachineStateChangedEventArgs> StateChanged;

        public StateMachineState AddOffState()
        {
            return AddState(BinaryActuatorState.Off.ToString());
        }

        public StateMachineState AddOnState()
        {
            return AddState(BinaryActuatorState.On.ToString());
        }

        public StateMachineState AddState(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var state = new StateMachineState(id, this);
            States.Add(state);
            return state;
        }

        public StateMachineState AddState()
        {
            string id = (States.Count + 1).ToString();
            return AddState(id);
        }

        public void SetState(string id, params IParameter[] parameters)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            string oldState = GetState();

            for (int i = 0; i < States.Count; i++)
            {
                var state = States[i];

                if (state.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    bool stateIsAlreadyActive = i == _index;
                    if (stateIsAlreadyActive && _turnOffIfStateIsAppliedTwice)
                    {
                        if (state.Id == BinaryActuatorState.Off.ToString())
                        {
                            // The state is already "Off".
                            return;
                        }

                        TurnOff();
                        return;
                    }

                    _index = i;
                    state.Apply(parameters);
                    StateChanged?.Invoke(this, new StateMachineStateChangedEventArgs(oldState, state.Id));
                    return;
                }
            }

            throw new NotSupportedException("StateMachineActuator '" + Id + "' does not support state '" + id + "'.");
        }

        public void SetNextState(params IParameter[] parameters)
        {
            string oldState = GetState();

            _index += 1;
            if (_index >= States.Count)
            {
                _index = 0;
            }

            string newState = States[_index].Id;

            States[_index].Apply(parameters);
            StateChanged?.Invoke(this, new StateMachineStateChangedEventArgs(oldState, newState));

            Logger.Info(Id + ": " + oldState + "->" + newState);
        }

        public void SetInitialState()
        {
            TurnOff(new ForceUpdateStateParameter());
        }

        public string GetState()
        {
            return States[_index].Id;
        }

        public void TurnOff(params IParameter[] parameters)
        {
            SetState(BinaryActuatorState.Off.ToString(), parameters);
        }

        public StateMachine WithTurnOffIfStateIsAppliedTwice()
        {
            _turnOffIfStateIsAppliedTwice = true;
            return this;
        }

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();

            if (States.Any())
            {
                status.SetNamedValue("state", States[_index].Id.ToJsonValue());
            }

            return status;
        }

        public override JsonObject ExportConfigurationToJsonObject()
        {
            JsonObject configuration = base.ExportConfigurationToJsonObject();

            JsonArray stateMachineStates = new JsonArray();
            foreach (var state in States)
            {
                stateMachineStates.Add(JsonValue.CreateStringValue(state.Id));
            }

            configuration.SetNamedValue("states", stateMachineStates);

            return configuration;
        }

        public override void HandleApiPost(ApiRequestContext context)
        {
            if (!context.Request.ContainsKey("state"))
            {
                return;
            }

            string stateId = context.Request.GetNamedString("state", string.Empty);
            SetState(stateId);
        }
    }
}