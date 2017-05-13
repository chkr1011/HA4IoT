using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Sensors.Events;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Triggers;

namespace HA4IoT.Sensors.Buttons
{
    public static class ButtonExtensions
    {
        public static IButton GetButton(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IButton>($"{area.Id}.{id}");
        }

        public static ITrigger CreatePressedShortTrigger(this IButton button, IMessageBrokerService messageBroker)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (messageBroker == null) throw new ArgumentNullException(nameof(messageBroker));

            return messageBroker.CreateTrigger<ButtonPressedShortEvent>(button.Id);
        }

        public static ITrigger CreatePressedLongTrigger(this IButton button, IMessageBrokerService messageBroker)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (messageBroker == null) throw new ArgumentNullException(nameof(messageBroker));

            return messageBroker.CreateTrigger<ButtonPressedLongEvent>(button.Id);
        }
    }
}
