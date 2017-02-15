using System;
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
    public class Fan : ActuatorBase, IFan
    {
        private readonly object _syncRoot = new object();
        private readonly IFanAdapter _adapter;

        private int _currentLevel;

        public Fan(ComponentId id, IFanAdapter adapter) : base(id)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            _adapter = adapter;

            SetNextLevelAction = new ActionWrapper(() => InvokeCommand(new IncreaseLevelCommand()));
        }

        public IAction SetNextLevelAction { get; }

        public override ComponentFeatureStateCollection GetState()
        {
            lock (_syncRoot)
            {
                var powerState = _currentLevel == 0 ? PowerStateValue.Off : PowerStateValue.On;

                return new ComponentFeatureStateCollection()
                    .With(new PowerState(powerState))
                    .With(new LevelState(_currentLevel));
            }
        }

        public override ComponentFeatureCollection GetFeatures()
        {
            lock (_syncRoot)
            {
                return new ComponentFeatureCollection()
                    .With(new PowerStateFeature())
                    .With(new LevelStateFeature { MaxLevel = _adapter.MaxLevel });
            }
        }

        public override void InvokeCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandInvoker = new CommandInvoker();
            commandInvoker.Register<TurnOffCommand>(c => SetLevelInternal(0));
            commandInvoker.Register<TurnOnCommand>(c => SetLevelInternal(_adapter.MaxLevel));
            commandInvoker.Register<SetLevelCommand>(c => SetLevelInternal(c.Level));
            commandInvoker.Register<IncreaseLevelCommand>(c => SetLevelInternal(_currentLevel+1));
            commandInvoker.Register<DecreaseLevelCommand>(c => SetLevelInternal(_currentLevel-1));
            commandInvoker.Invoke(command);
        }

        public override void ChangeState(IComponentFeatureState state, params IHardwareParameter[] parameters)
        {
            // TODO: Delete!
        }

        public override void ResetState()
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
                    _adapter.SetLevel(level);
                }
                else
                {
                    _adapter.SetLevel(level, HardwareParameter.ForceUpdateState);
                }

                _currentLevel = level;

                var newState = GetState();

                OnStateChanged(oldState, newState);
            }
        }
    }
}
