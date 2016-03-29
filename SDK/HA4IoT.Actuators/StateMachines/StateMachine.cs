using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Actuators.Parameters;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class StateMachine : ActuatorBase, IStateMachine
    {
        private readonly Dictionary<StateMachineStateId, StateMachineStateId> _stateAlias = new Dictionary<StateMachineStateId, StateMachineStateId>(); 
        private readonly List<IStateMachineState> _states = new List<IStateMachineState>(); 

        private IStateMachineState _activeState;
        private bool _turnOffIfStateIsAppliedTwice;

        public StateMachine(ActuatorId id)
            : base(id)
        {
        }

        public event EventHandler<StateMachineStateChangedEventArgs> ActiveStateChanged;

        public bool GetSupportsState(StateMachineStateId stateId)
        {
            if (stateId == null) throw new ArgumentNullException(nameof(stateId));

            if (_states.Any(s => s.Id.Equals(stateId)))
            {
                return true;
            }

            if (!_stateAlias.TryGetValue(stateId, out stateId))
            {
                return false;
            }

            return _states.Any(s => s.Id.Equals(stateId));
        }

        public void SetActiveState(StateMachineStateId id, params IHardwareParameter[] parameters)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            ThrowIfNoStatesAvailable();

            IStateMachineState oldState = _activeState;
            IStateMachineState newState = GetState(id);

            if (newState.Id.Equals(_activeState?.Id))
            {
                if (_turnOffIfStateIsAppliedTwice && GetSupportsState(DefaultStateIDs.Off) && !GetActiveState().Equals(DefaultStateIDs.Off))
                {
                    SetActiveState(DefaultStateIDs.Off, parameters);
                    return;
                }

                if (!parameters.Any(p => p is ForceUpdateStateParameter))
                {
                    return;
                }
            }

            oldState?.Deactivate(parameters);
            newState.Activate(parameters);

            _activeState = newState;
            OnActiveStateChanged(oldState);
        }

        public void SetStateIdAlias(StateMachineStateId stateId, StateMachineStateId alias)
        {
            _stateAlias[alias] = stateId;
        }

        public StateMachineStateId GetActiveState()
        {
            ThrowIfNoStatesAvailable();

            return _activeState?.Id;
        }

        public override JsonObject ExportStatusToJsonObject()
        {
            var status = base.ExportStatusToJsonObject();

            if (_activeState != null)
            {
                status.SetNamedString("state", _activeState.Id.Value);
            }

            return status;
        }

        public override JsonObject ExportConfigurationToJsonObject()
        {
            JsonObject configuration = base.ExportConfigurationToJsonObject();

            JsonArray stateMachineStates = new JsonArray();
            foreach (var state in _states)
            {
                stateMachineStates.Add(JsonValue.CreateStringValue(state.Id.Value));
            }

            configuration.SetNamedValue("states", stateMachineStates);

            return configuration;
        }

        public StateMachineStateId GetNextState(StateMachineStateId stateId)
        {
            if (stateId == null) throw new ArgumentNullException(nameof(stateId));

            ThrowIfStateNotSupported(stateId);

            IStateMachineState startState = GetState(stateId);

            int indexOfStartState = _states.IndexOf(startState);
            if (indexOfStartState == _states.Count - 1)
            {
                return _states.First().Id;
            }

            return _states[indexOfStartState + 1].Id;
        }

        public StateMachine WithTurnOffIfStateIsAppliedTwice()
        {
            _turnOffIfStateIsAppliedTwice = true;
            return this;
        }

        public virtual void SetInitialState(StateMachineStateId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            SetActiveState(id, new ForceUpdateStateParameter());
        }

        public void AddState(IStateMachineState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            if (GetSupportsState(state.Id))
            {
                throw new InvalidOperationException($"State '{state.Id}' already added.");
            }

            _states.Add(state);
        }

        protected override void HandleApiCommand(IApiContext apiContext)
        {
            if (!apiContext.Request.ContainsKey("state"))
            {
                return;
            }

            var stateId = new StateMachineStateId(apiContext.Request.GetNamedString("state", string.Empty));
            if (!GetSupportsState(stateId))
            {
                apiContext.ResultCode = ApiResultCode.InvalidBody;
                apiContext.Response.SetNamedString("Message", "State ID not supported.");
            }

            SetActiveState(stateId);
        }

        private IStateMachineState GetState(StateMachineStateId id)
        {
            IStateMachineState state = _states.FirstOrDefault(s => s.Id.Equals(id));

            if (state == null && _stateAlias.TryGetValue(id, out id))
            {
                state = _states.FirstOrDefault(s => s.Id.Equals(id));
            }

            if (state == null)
            {
                throw new InvalidOperationException("State machine state is unknown.");
            }

            return state;
        }

        private void OnActiveStateChanged(IStateMachineState oldState)
        {
            Log.Info(Id + ": " + oldState?.Id + "->" + _activeState.Id);

            ActiveStateChanged?.Invoke(this, new StateMachineStateChangedEventArgs(oldState?.Id, _activeState.Id));
            NotifyStateChanged();
        }

        private void ThrowIfNoStatesAvailable()
        {
            if (!_states.Any())
            {
                throw new InvalidOperationException("The State Machine does not support any state.");
            }
        }

        private void ThrowIfStateNotSupported(StateMachineStateId stateId)
        {
            if (!GetSupportsState(stateId))
            {
                throw new NotSupportedException($"State '{stateId}' is not supported.");
            }
        }
    }
}