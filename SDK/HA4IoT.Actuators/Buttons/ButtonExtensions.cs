using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public static class ButtonExtensions
    {
        public static IArea WithButton(this IArea room, Enum id, IBinaryInput input)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (input == null) throw new ArgumentNullException(nameof(input));

            room.AddActuator(new Button(ActuatorIdFactory.Create(room, id), input, room.Controller.HttpApiController, room.Controller.Logger, room.Controller.Timer));
            return room;
        }

        public static IButton WithPressedShortlyAction(this IButton button, Action action)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (action == null) throw new ArgumentNullException(nameof(action));

            button.GetPressedShortlyTrigger().Attach(action);
            return button;
        }

        public static IButton WithPressedLongAction(this IButton button, Action action)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (action == null) throw new ArgumentNullException(nameof(action));

            button.GetPressedLongTrigger().Attach(action);
            return button;
        }

        public static IArea WithRollerShutterButtons(this IArea room, Enum id, IBinaryInput upInput, IBinaryInput downInput)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            var rollerShutterButtons = new RollerShutterButtons(ActuatorIdFactory.Create(room, id), upInput, downInput,
                room.Controller.HttpApiController, room.Controller.Logger, room.Controller.Timer);

            room.AddActuator(rollerShutterButtons);
            return room;
        }

        public static IArea WithVirtualButton(this IArea room, Enum id, Action<VirtualButton> initializer)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var virtualButton = new VirtualButton(ActuatorIdFactory.Create(room, id), room.Controller.HttpApiController, room.Controller.Logger);
            initializer.Invoke(virtualButton);

            room.AddActuator(virtualButton);
            return room;
        }

        public static IArea WithVirtualButtonGroup(this IArea room, Enum id, Action<VirtualButtonGroup> initializer)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var virtualButtonGroup = new VirtualButtonGroup(ActuatorIdFactory.Create(room, id), room.Controller.HttpApiController, room.Controller.Logger);
            initializer.Invoke(virtualButtonGroup);

            room.AddActuator(virtualButtonGroup);
            return room;
        }

        public static IButton GetButton(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetActuator<IButton>(ActuatorIdFactory.Create(room, id));
        }

        public static IRollerShutterButtons GetRollerShutterButtons(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetActuator<RollerShutterButtons>(ActuatorIdFactory.Create(room, id));
        }
    }
}
