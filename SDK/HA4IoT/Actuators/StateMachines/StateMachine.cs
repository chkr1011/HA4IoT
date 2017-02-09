using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.StateMachines
{
    public class StateMachine : ActuatorBase, IStateMachine
    {
        private readonly Dictionary<IComponentFeatureState, IComponentFeatureState> _stateAlias = new Dictionary<IComponentFeatureState, IComponentFeatureState>();
        private readonly List<IStateMachineState> _states = new List<IStateMachineState>();

        private IStateMachineState _activeState;
        private bool _turnOffIfStateIsAppliedTwice;

        public StateMachine(ComponentId id)
            : base(id)
        {
        }

        public bool SupportsState(IComponentFeatureState stateId)
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

        public override void ChangeState(IComponentFeatureState id, params IHardwareParameter[] parameters)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            ThrowIfNoStatesAvailable();

            IStateMachineState oldState = _activeState;
            IStateMachineState newState = GetState(id);

            if (newState.Id.Equals(_activeState?.Id))
            {
                if (_turnOffIfStateIsAppliedTwice && SupportsState(BinaryStateId.Off) && !GetState().Equals(BinaryStateId.Off))
                {
                    ChangeState(BinaryStateId.Off, parameters);
                    return;
                }

                if (!parameters.Any(p => p is ForceUpdateStateParameter))
                {
                    return;
                }
            }

            oldState?.Deactivate(parameters);
            newState.Activate(parameters);

            if (parameters.Any(p => p is IsPartOfPartialUpdateParameter))
            {
                return;
            }

            _activeState = newState;
            OnActiveStateChanged(oldState, newState);
        }

        public override void ResetState()
        {
            if (SupportsState(BinaryStateId.Off))
            {
                ChangeState(BinaryStateId.Off, new ForceUpdateStateParameter());
            }
        }

        public void SetStateIdAlias(GenericComponentState stateId, GenericComponentState alias)
        {
            _stateAlias[alias] = stateId;
        }

        public override ComponentFeatureStateCollection GetState()
        {
            ThrowIfNoStatesAvailable();

            if (_activeState == null)
            {
                return new ComponentFeatureStateCollection();
            }

            return new ComponentFeatureStateCollection().With(_activeState?.Id);
        }

        public override ComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection();
        }

        public override void InvokeCommand(ICommand command)
        {
            
        }

        public GenericComponentState GetNextState(IComponentFeatureState stateId)
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

        public void SetInitialState(GenericComponentState id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            ChangeState(id, HardwareParameter.ForceUpdateState);
        }

        public void AddState(IStateMachineState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            if (SupportsState(state.Id))
            {
                throw new InvalidOperationException($"State '{state.Id}' already added.");
            }

            _states.Add(state);
        }

        public override void HandleApiCall(IApiContext apiContext)
        {
            var request = apiContext.Parameter.ToObject<ApiCallRequest>();

            if (!string.IsNullOrEmpty(request.Action))
            {
                // TODO: Check whether required.
                ////if (request.Action == "nextState")
                ////{
                ////    ChangeState(GetNextState(GetState().First()));
                ////}

                return;
            }

            if (!string.IsNullOrEmpty(request.State))
            {
                var stateId = new GenericComponentState(request.State);
                if (!SupportsState(stateId))
                {
                    apiContext.ResultCode = ApiResultCode.InvalidParameter;
                    apiContext.Result["Message"] = "State ID not supported.";
                }

                ChangeState(stateId);
            }
        }

        public override IList<GenericComponentState> GetSupportedStates()
        {
            return _states.Select(s => s.Id).ToList();
        }

        private IStateMachineState GetState(IComponentFeatureState id)
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

        protected virtual void OnActiveStateChanged(IStateMachineState oldState, IStateMachineState newState)
        {
            OnStateChanged(oldState?.Id, newState.Id);
        }

        private void ThrowIfNoStatesAvailable()
        {
            if (!_states.Any())
            {
                throw new InvalidOperationException("The State Machine does not support any state.");
            }
        }

        private void ThrowIfStateNotSupported(IComponentFeatureState stateId)
        {
            if (!SupportsState(stateId))
            {
                throw new NotSupportedException($"State '{stateId}' is not supported.");
            }
        }
    }
}