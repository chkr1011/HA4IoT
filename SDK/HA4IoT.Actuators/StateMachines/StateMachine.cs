using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Actuators.Actions;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class StateMachine : ActuatorBase<ActuatorSettings>, IStateMachine
    {
        private readonly IHomeAutomationAction _turnOffAction;
        private readonly IHomeAutomationAction _setNextStateAction;

        private int _index;
        private bool _turnOffIfStateIsAppliedTwice;

        public StateMachine(ActuatorId id, IApiController apiController)
            : base(id, apiController)
        {
            Settings = new ActuatorSettings(id);

            _turnOffAction = new HomeAutomationAction(() => TurnOff());
            _setNextStateAction = new HomeAutomationAction(() => SetNextState());
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

        public void SetState(string id, params IHardwareParameter[] parameters)
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

                    OnStateChanged(oldState, state.Id);
                    return;
                }
            }

            throw new NotSupportedException("StateMachineActuator '" + Id + "' does not support state '" + id + "'.");
        }

        public void SetNextState(params IHardwareParameter[] parameters)
        {
            string oldState = GetState();

            _index += 1;
            if (_index >= States.Count)
            {
                _index = 0;
            }

            string newState = States[_index].Id;

            States[_index].Apply(parameters);

            OnStateChanged(oldState, newState);
        }

        public void SetInitialState()
        {
            TurnOff(new ForceUpdateStateParameter());
        }

        public string GetState()
        {
            return States[_index].Id;
        }

        public IHomeAutomationAction GetTurnOffAction()
        {
            return _turnOffAction;
        }

        public IHomeAutomationAction GetSetNextStateAction()
        {
            return _setNextStateAction;
        }

        public IHomeAutomationAction GetSetStateAction(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            
            return new HomeAutomationAction(() => SetState(id));
        }

        public void TurnOff(params IHardwareParameter[] parameters)
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

        protected override void HandleApiCommand(IApiContext apiContext)
        {
            if (!apiContext.Request.ContainsKey("state"))
            {
                return;
            }

            string stateId = apiContext.Request.GetNamedString("state", string.Empty);
            SetState(stateId);
        }

        private void OnStateChanged(string oldState, string newState)
        {
            StateChanged?.Invoke(this, new StateMachineStateChangedEventArgs(oldState, newState));
            ApiController.NotifyStateChanged(this);

            Log.Info(Id + ": " + oldState + "->" + newState);
        }
    }
}