using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public static class ButtonExtensions
    {
        public static Room WithButton(this Room room, Enum id, IBinaryInput input)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (input == null) throw new ArgumentNullException(nameof(input));

            return room.WithActuator(id,
                new Button(room.GenerateActuatorId(id), input, room.Home.HttpApiController,
                    room.Home.NotificationHandler, room.Home.Timer));
        }

        public static Room WithRollerShutterButtons(this Room room, Enum id, IBinaryInput upInput, IBinaryInput downInput)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            return room.WithActuator(id,
                new RollerShutterButtons(room.GenerateActuatorId(id), upInput, downInput, room.Home.HttpApiController,
                    room.Home.NotificationHandler, room.Home.Timer));
        }

        public static Room WithVirtualButton(this Room room, Enum id, Action<VirtualButton> initializer)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var virtualButton = new VirtualButton(room.GenerateActuatorId(id), room.Home.HttpApiController, room.Home.NotificationHandler);
            initializer.Invoke(virtualButton);

            return room.WithActuator(id, virtualButton);
        }

        public static Room WithVirtualButtonGroup(this Room room, Enum id, Action<VirtualButtonGroup> initializer)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var virtualButtonGroup = new VirtualButtonGroup(room.GenerateActuatorId(id), room.Home.HttpApiController, room.Home.NotificationHandler);
            initializer.Invoke(virtualButtonGroup);

            return room.WithActuator(id, virtualButtonGroup);
        }

        public static Button Button(this Room room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.Actuator<Button>(id);
        }

        public static RollerShutterButtons RollerShutterButtons(this Room room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.Actuator<RollerShutterButtons>(id);
        }
    }
}
