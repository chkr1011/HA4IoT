using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors.Buttons
{
    public static class ButtonExtensions
    {
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
