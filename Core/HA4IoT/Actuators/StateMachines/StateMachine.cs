using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Commands;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Actuators.StateMachines
{
    public class StateMachine : ComponentBase, IStateMachine
    {
        private readonly Dictionary<string, IStateMachineState> _states = new Dictionary<string, IStateMachineState>();

        private IStateMachineState _activeState;
        private bool _turnOffIfStateIsAppliedTwice;

        public StateMachine(string id) : base(id)
        {
        }

        public string ResetStateId { get; set; }

        ////public bool SupportsState(IComponentFeatureState stateId)
        ////{
        ////    if (stateId == null) throw new ArgumentNullException(nameof(stateId));

        ////    if (_states.Any(s => s.Id.Equals(stateId)))
        ////    {
        ////        return true;
        ////    }

        ////    if (!_stateAlias.TryGetValue(stateId, out stateId))
        ////    {
        ////        return false;
        ////    }

        ////    return _states.Any(s => s.Id.Equals(stateId));
        ////}

        ////public void ChangeState(string id, params IHardwareParameter[] parameters)
        ////{
        ////    if (id == null) throw new ArgumentNullException(nameof(id));

        ////    ThrowIfNoStatesAvailable();

        ////    IStateMachineState oldState = _activeState;
        ////    IStateMachineState newState = GetState(id);

        ////    if (newState.Id.Equals(_activeState?.Id))
        ////    {
        ////        if (_turnOffIfStateIsAppliedTwice && SupportsState(BinaryStateId.Off) && !GetState().Equals(BinaryStateId.Off))
        ////        {
        ////            ChangeState(BinaryStateId.Off, parameters);
        ////            return;
        ////        }

        ////        if (!parameters.Any(p => p is ForceUpdateStateParameter))
        ////        {
        ////            return;
        ////        }
        ////    }

        ////    oldState?.Deactivate(parameters);
        ////    newState.Activate(parameters);

        ////    if (parameters.Any(p => p is IsPartOfPartialUpdateParameter))
        ////    {
        ////        return;
        ////    }

        ////    _activeState = newState;
        ////    OnActiveStateChanged(oldState, newState);
        ////}

        public void ResetState()
        {
            if (SupportsState(ResetStateId))
            {
                SetState(ResetStateId, new ForceUpdateStateParameter());
            }
            else
            {
                Log.Default.Warning("Reset stat of StateMachine is not supported.");
            }
        }

        public override IComponentFeatureStateCollection GetState()
        {
            var state = new ComponentFeatureStateCollection().With(
                new StateMachineFeatureState(_activeState?.Id));

            if (this.GetSupportsOffState())
            {
                state.With(
                    new PowerState(_activeState?.Id == StateMachineStateExtensions.OffStateId
                        ? PowerStateValue.Off
                        : PowerStateValue.On));
            }

            return state;
        }

        public override IComponentFeatureCollection GetFeatures()
        {
            var stateMachineFeature = new StateMachineFeature();
            foreach (var stateId in _states.Keys)
            {
                stateMachineFeature.SupportedStates.Add(stateId);
            }
            
            return new ComponentFeatureCollection()
                .With(stateMachineFeature);
        }

        public override void ExecuteCommand(ICommand command)
        {
            var commandExecutor = new CommandExecutor();
            commandExecutor.Register<ResetCommand>(c => ResetState());
            commandExecutor.Register<SetStateCommand>(c => SetState(c.Id));
            commandExecutor.Execute(command);
        }

        public StateMachine WithTurnOffIfStateIsAppliedTwice()
        {
            _turnOffIfStateIsAppliedTwice = true;
            return this;
        }

        public void AddState(IStateMachineState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            _states.Add(state.Id, state);
        }
        
        public bool SupportsState(string id)
        {
            return _states.ContainsKey(id);
        }

        private void SetState(string id, params IHardwareParameter[] parameters)
        {
            ThrowIfNoStatesAvailable();
            ThrowIfStateNotSupported(id);
            
            _activeState?.Deactivate(parameters);
            _activeState = _states[id];
            _activeState.Activate(parameters);
        }

        protected virtual void OnActiveStateChanged(IStateMachineState oldState, IStateMachineState newState)
        {
            //OnStateChanged(oldState?.Id, newState.Id);
        }

        private void ThrowIfNoStatesAvailable()
        {
            if (!_states.Any())
            {
                throw new InvalidOperationException("StateMachine does not support any state.");
            }
        }

        private void ThrowIfStateNotSupported(string id)
        {
            if (!SupportsState(id))
            {
                throw new NotSupportedException($"StateMachine state '{id}' is not supported.");
            }
        }
    }
}