using System;
using HA4IoT.Actuators.Fans;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Adapters;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Actuators
{
    public class ActuatorFactory
    {
        private readonly ILogService _logService;
        private readonly ITimerService _timerService;
        private readonly ISettingsService _settingsService;

        public ActuatorFactory(ITimerService timerService, ISettingsService settingsService, ILogService logService)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public IStateMachine RegisterStateMachine(IArea area, Enum id, Action<StateMachine, IArea> initializer = null)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            
            var stateMachine = new StateMachine($"{area.Id}.{id}", _logService);
            initializer?.Invoke(stateMachine, area);

            area.RegisterComponent(stateMachine);
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

            area.RegisterComponent(rollerShutter);

            return rollerShutter;
        }

        public ISocket RegisterSocket(IArea area, Enum id, IBinaryOutput output)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (output == null) throw new ArgumentNullException(nameof(output));

            var socket = new Socket($"{area.Id}.{id}", new BinaryOutputAdapter(output));
            area.RegisterComponent(socket);

            return socket;
        }

        public ILamp RegisterLamp(IArea area, Enum id, ILampAdapter adapter)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            var lamp = new Lamp($"{area.Id}.{id}", adapter);
            area.RegisterComponent(lamp);

            return lamp;
        }

        public ILamp RegisterLamp(IArea area, Enum id, IBinaryOutput output)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (output == null) throw new ArgumentNullException(nameof(output));

            return RegisterLamp(area, id, new BinaryOutputAdapter(output));
        }

        public IFan RegisterFan(IArea area, Enum id, IFanAdapter adapter)
        {
            var fan = new Fan($"{area.Id}.{id}", adapter, _settingsService);
            area.RegisterComponent(fan);
            return fan;
        }

        public LogicalComponent RegisterLogicalComponent(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var component = new LogicalComponent($"{area.Id}.{id}");
            area.RegisterComponent(component);

            return component;
        }
    }
}
