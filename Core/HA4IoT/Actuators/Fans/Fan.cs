using System;
using HA4IoT.Commands;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.Fans
{
    public class Fan : ComponentBase, IFan
    {
        private readonly object _syncRoot = new object();
        private readonly IFanAdapter _adapter;

        private int _currentLevel;

        public Fan(string id, IFanAdapter adapter) : base(id)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

            SetNextLevelAction = new ActionWrapper(() => ExecuteCommand(new IncreaseLevelCommand()));
        }

        public IAction SetNextLevelAction { get; }

        public override IComponentFeatureStateCollection GetState()
        {
            lock (_syncRoot)
            {
                var powerState = _currentLevel == 0 ? PowerStateValue.Off : PowerStateValue.On;

                return new ComponentFeatureStateCollection()
                    .With(new PowerState(powerState))
                    .With(new LevelState(_currentLevel));
            }
        }

        public override IComponentFeatureCollection GetFeatures()
        {
            lock (_syncRoot)
            {
                return new ComponentFeatureCollection()
                    .With(new PowerStateFeature())
                    .With(new LevelStateFeature { MaxLevel = _adapter.MaxLevel });
            }
        }

        public override void ExecuteCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandExecutor = new CommandExecutor();
            commandExecutor.Register<ResetCommand>(c => ResetState());
            commandExecutor.Register<TurnOffCommand>(c => SetLevelInternal(0));
            commandExecutor.Register<TurnOnCommand>(c => SetLevelInternal(_adapter.MaxLevel));
            commandExecutor.Register<SetLevelCommand>(c => SetLevelInternal(c.Level));
            commandExecutor.Register<IncreaseLevelCommand>(c => SetLevelInternal(_currentLevel+1));
            commandExecutor.Register<DecreaseLevelCommand>(c => SetLevelInternal(_currentLevel-1));
            commandExecutor.Execute(command);
        }

        public void ResetState()
        {
            SetLevelInternal(0, true);
        }

        private void SetLevelInternal(int level, bool forceUpdate = false)
        {
            if (level < 0)
            {
                level = _adapter.MaxLevel;
            }

            if (level > _adapter.MaxLevel)
            {
                level = 0;
            }

            lock (_syncRoot)
            {
                if (!forceUpdate && _currentLevel == level)
                {
                    return;
                }

                var oldState = GetState();

                if (!forceUpdate)
                {
                    _adapter.SetState(level);
                }
                else
                {
                    _adapter.SetState(level, HardwareParameter.ForceUpdateState);
                }

                _currentLevel = level;

                var newState = GetState();

                OnStateChanged(oldState, newState);
            }
        }
    }
}
