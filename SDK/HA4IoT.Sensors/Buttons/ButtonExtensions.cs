using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors.Buttons
{
    public static class ButtonExtensions
    {
        public static IArea WithVirtualButton(this IArea area, Enum id, Action<IButton> initializer = null)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            
            var virtualButton = new Button(ComponentIdFactory.Create(area.Id, id), new EmptyButtonEndpoint(), area.Controller.Timer);
            initializer?.Invoke(virtualButton);

            area.AddComponent(virtualButton);
            return area;
        }

        public static IArea WithButton(this IArea area, Enum id, IBinaryInput input, Action<IButton> initializer = null)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var button = new Button(
                ComponentIdFactory.Create(area.Id, id), 
                new PortBasedButtonEndpoint(input),
                area.Controller.Timer);

            initializer?.Invoke(button);

            area.AddComponent(button);
            return area;
        }

        public static IArea WithRollerShutterButtons(
            this IArea area, 
            Enum upId,
            IBinaryInput upInput,
            Enum downId, 
            IBinaryInput downInput)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            if (upInput == null) throw new ArgumentNullException(nameof(upInput));
            if (downInput == null) throw new ArgumentNullException(nameof(downInput));

            var upButton = new Button(
                ComponentIdFactory.Create(area.Id, upId),
                new PortBasedButtonEndpoint(upInput), 
                area.Controller.Timer);

            area.AddComponent(upButton);

            var downButton = new Button(
                ComponentIdFactory.Create(area.Id, downId),
                new PortBasedButtonEndpoint(downInput), 
                area.Controller.Timer);

            area.AddComponent(downButton);

            return area;
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

        public static IButton GetButton(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IButton>(ComponentIdFactory.Create(area.Id, id));
        }
    }
}
