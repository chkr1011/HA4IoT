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
        private readonly object _syncRoot = new object();

        private readonly CommandExecutor _commandExecutor = new CommandExecutor();
        private readonly Dictionary<string, IStateMachineState> _states = new Dictionary<string, IStateMachineState>();
        private IStateMachineState _activeState;

        public StateMachine(string id) : base(id)
        {
            _commandExecutor.Register<ResetCommand>(c => ResetState());
            _commandExecutor.Register<SetStateCommand>(c => SetState(c.Id));

            if (SupportsState(StateMachineStateExtensions.OnStateId))
            {
                _commandExecutor.Register<TurnOnCommand>(c => SetState(StateMachineStateExtensions.OnStateId));
            }

            if (SupportsState(StateMachineStateExtensions.OffStateId))
            {
                _commandExecutor.Register<TurnOffCommand>(c => SetState(StateMachineStateExtensions.OffStateId));
            }
        }

        public string AlternativeStateId { get; set; }

        public string ResetStateId { get; set; }

        public void RegisterState(IStateMachineState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            lock (_syncRoot)
            {
                _states.Add(state.Id, state);
            }
        }

        public void ResetState()
        {
            lock (_syncRoot)
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
            lock (_syncRoot)
            {
                _commandExecutor.Execute(command);
            }
        }

        public bool SupportsState(string id)
        {
            lock (_syncRoot)
            {
                return _states.ContainsKey(id);
            }
        }

        private void SetState(string id, params IHardwareParameter[] parameters)
        {
            ThrowIfNoStatesAvailable();
            ThrowIfStateNotSupported(id);

            var oldState = GetState();

            if (AlternativeStateId != null && id == _activeState?.Id)
            {
                id = AlternativeStateId;
            }

            _activeState?.Deactivate(parameters);
            _activeState = _states[id];
            _activeState.Activate(parameters);

            OnStateChanged(oldState);
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