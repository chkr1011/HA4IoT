using System;
using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Actuators.RollerShutters;
using HA4IoT.Actuators.Sockets;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Actuators
{
    public class ActuatorFactory
    {
        private readonly ITimerService _timerService;
        private readonly ISchedulerService _schedulerService;
        private readonly ISettingsService _settingsService;

        public ActuatorFactory(ITimerService timerService, ISchedulerService schedulerService, ISettingsService settingsService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _timerService = timerService;
            _schedulerService = schedulerService;
            _settingsService = settingsService;
        }

        public IStateMachine RegisterStateMachine(IArea area, Enum id, Action<StateMachine, IArea> initializer)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var stateMachine = new StateMachine(ComponentIdFactory.Create(area.Id, id));

            initializer(stateMachine, area);
            stateMachine.SetInitialState(BinaryStateId.Off);

            area.AddComponent(stateMachine);
            return stateMachine;
        }

        public IRollerShutter RegisterRollerShutter(IArea area, Enum id, IBinaryOutput powerOutput, IBinaryOutput directionOutput)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (powerOutput == null) throw new ArgumentNullException(nameof(powerOutput));
            if (directionOutput == null) throw new ArgumentNullException(nameof(directionOutput));

            var rollerShutter = new RollerShutter(
                ComponentIdFactory.Create(area.Id, id),
                new PortBasedRollerShutterEndpoint(powerOutput, directionOutput),
                _timerService,
                _schedulerService,
                _settingsService);

            area.AddComponent(rollerShutter);

            return rollerShutter;
        }

        public ISocket RegisterSocket(IArea area, Enum id, IBinaryOutput output)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (output == null) throw new ArgumentNullException(nameof(output));

            var socket = new Socket(ComponentIdFactory.Create(area.Id, id), new PortBasedBinaryStateEndpoint(output));
            area.AddComponent(socket);

            return socket;
        }

        public ILamp RegisterLamp(IArea area, Enum id, IBinaryOutput output)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (output == null) throw new ArgumentNullException(nameof(output));

            var lamp = new Lamp(ComponentIdFactory.Create(area.Id, id), new PortBasedBinaryStateEndpoint(output));
            area.AddComponent(lamp);

            return lamp;
        }

        public LogicalBinaryStateActuator RegisterLogicalActuator(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var actuator = new LogicalBinaryStateActuator(ComponentIdFactory.Create(area.Id, id), _timerService);
            area.AddComponent(actuator);

            return actuator;
        }
    }
}
