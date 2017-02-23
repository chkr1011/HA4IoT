using System;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Adapters;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Actuators
{
    public class ActuatorFactory
    {
        private readonly ITimerService _timerService;
        private readonly ISettingsService _settingsService;

        public ActuatorFactory(ITimerService timerService, ISettingsService settingsService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _timerService = timerService;
            _settingsService = settingsService;
        }

        public IStateMachine RegisterStateMachine(IArea area, Enum id, Action<StateMachine, IArea> initializer)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var stateMachine = new StateMachine($"{area.Id}.{id}");

            initializer(stateMachine, area);
            area.AddComponent(stateMachine);
            return stateMachine;
        }

        public IRollerShutter RegisterRollerShutter(IArea area, Enum id, IBinaryOutput powerOutput, IBinaryOutput directionOutput)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (powerOutput == null) throw new ArgumentNullException(nameof(powerOutput));
            if (directionOutput == null) throw new ArgumentNullException(nameof(directionOutput));

            var rollerShutter = new RollerShutter(
                $"{area.Id}.{id}",
                new PortBasedRollerShutterAdapter(powerOutput, directionOutput), 
                _timerService,
                _settingsService);

            area.AddComponent(rollerShutter);

            return rollerShutter;
        }

        public ISocket RegisterSocket(IArea area, Enum id, IBinaryOutput output)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (output == null) throw new ArgumentNullException(nameof(output));

            var socket = new Socket($"{area.Id}.{id}", new BinaryOutputAdapter(output));
            area.AddComponent(socket);

            return socket;
        }

        public ILamp RegisterLamp(IArea area, Enum id, IBinaryOutput output)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (output == null) throw new ArgumentNullException(nameof(output));

            var lamp = new Lamp($"{area.Id}.{id}", new BinaryOutputAdapter(output));
            area.AddComponent(lamp);

            return lamp;
        }

        public LogicalComponent RegisterLogicalComponent(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var component = new LogicalComponent($"{area.Id}.{id}");
            area.AddComponent(component);

            return component;
        }
    }
}
