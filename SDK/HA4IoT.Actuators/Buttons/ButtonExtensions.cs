using System;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public static class ButtonExtensions
    {
        public static IRoom WithButton(this IRoom room, Enum id, IBinaryInput input)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (input == null) throw new ArgumentNullException(nameof(input));

            room.AddActuator(new Button(ActuatorIdFactory.Create(room, id), input, room.Controller.HttpApiController, room.Controller.Logger, room.Controller.Timer));
            return room;
        }

        public static IRoom WithRollerShutterButtons(this IRoom room, Enum id, IBinaryInput upInput, IBinaryInput downInput)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            var rollerShutterButtons = new RollerShutterButtons(ActuatorIdFactory.Create(room, id), upInput, downInput,
                room.Controller.HttpApiController, room.Controller.Logger, room.Controller.Timer);

            room.AddActuator(rollerShutterButtons);
            return room;
        }

        public static IRoom WithVirtualButton(this IRoom room, Enum id, Action<VirtualButton> initializer)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var virtualButton = new VirtualButton(ActuatorIdFactory.Create(room, id), room.Controller.HttpApiController, room.Controller.Logger);
            initializer.Invoke(virtualButton);

            room.AddActuator(virtualButton);
            return room;
        }

        public static IRoom WithVirtualButtonGroup(this IRoom room, Enum id, Action<VirtualButtonGroup> initializer)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var virtualButtonGroup = new VirtualButtonGroup(ActuatorIdFactory.Create(room, id), room.Controller.HttpApiController, room.Controller.Logger);
            initializer.Invoke(virtualButtonGroup);

            room.AddActuator(virtualButtonGroup);
            return room;
        }

        public static Button Button(this IRoom room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.Actuator<Button>(ActuatorIdFactory.Create(room, id));
        }

        public static RollerShutterButtons RollerShutterButtons(this IRoom room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.Actuator<RollerShutterButtons>(ActuatorIdFactory.Create(room, id));
        }
    }
}
