using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Components;
using HA4IoT.Components.Commands;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Commands;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Actuators.StateMachines
{
    public class StateMachine : ComponentBase, IStateMachine
    {
        private readonly object _syncRoot = new object();

        private readonly List<IStateMachineState> _states = new List<IStateMachineState>();
        private readonly ILogger _log;

        private IStateMachineState _activeState;

        public StateMachine(string id, ILogService logService) 
            : base(id)
        {
            _log = logService.CreatePublisher(nameof(StateMachine));
        }

        public string AlternativeStateId { get; set; }

        public string ResetStateId { get; set; }

        public void RegisterState(IStateMachineState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            lock (_syncRoot)
            {
                ThrowIfStateAlreadySupported(state.Id);
                _states.Add(state);
            }
        }

        public void ResetState()
        {
            lock (_syncRoot)
            {
                if (string.IsNullOrEmpty(ResetStateId))
                {
                    _log.Warning($"Reset state not supported for component '{Id}'.");
                    return;
                }

                if (SupportsState(ResetStateId))
                {
                    SetState(ResetStateId, new ForceUpdateStateParameter());
                }
                else
                {
                    _log.Warning($"Defined reset state of component '{Id}' is not supported.");
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
            foreach (var state in _states)
            {
                stateMachineFeature.SupportedStates.Add(state.Id);
            }
            
            var features = new ComponentFeatureCollection()
                .With(stateMachineFeature);

            if (SupportsState(StateMachineStateExtensions.OffStateId) &&
                SupportsState(StateMachineStateExtensions.OnStateId))
            {
                features.With(new PowerStateFeature());
            }

            return features;
        }

        public override void ExecuteCommand(ICommand command)
        {
            lock (_syncRoot)
            {
                var commandExecutor = new CommandExecutor();
                commandExecutor.Register<ResetCommand>(c => ResetState());
                commandExecutor.Register<SetStateCommand>(c => SetState(c.Id));
                commandExecutor.Register<SetNextStateCommand>(c => SetNextState());

                if (SupportsState(StateMachineStateExtensions.OnStateId))
                {
                    commandExecutor.Register<TurnOnCommand>(c => SetState(StateMachineStateExtensions.OnStateId));
                }

                if (SupportsState(StateMachineStateExtensions.OffStateId))
                {
                    commandExecutor.Register<TurnOffCommand>(c => SetState(StateMachineStateExtensions.OffStateId));
                }

                commandExecutor.Execute(command);
            }
        }

        public bool SupportsState(string id)
        {
            lock (_syncRoot)
            {
                return _states.Any(s => s.Id.Equals(id));
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
            _activeState = _states.First(s => s.Id.Equals(id));
            _activeState.Activate(parameters);

            OnStateChanged(oldState);
        }

        private void SetNextState(params IHardwareParameter[] parameters)
        {
            var index = _states.FindIndex(s => s.Id.Equals(_activeState.Id));
            index++;

            if (index >= _states.Count)
            {
                index = 0;
            }

            SetState(_states[index].Id, parameters);
        }

        private void ThrowIfNoStatesAvailable()
        {
            if (!_states.Any())
            {
                throw new InvalidOperationException("StateMachine does not support any state.");
            }
        }

        private void ThrowIfStateAlreadySupported(string id)
        {
            if (SupportsState(id))
            {
                throw new InvalidOperationException($"StateMachine state '{id}' is aleady supported.");
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