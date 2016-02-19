using System;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Actuators.Connectors
{
    public static class RollerShutterWithRollerShutterButtonsConnector
    {
        public static IRollerShutter ConnectWith(this IRollerShutter rollerShutter, RollerShutterButtons buttons)
        {
            if (rollerShutter == null) throw new ArgumentNullException(nameof(rollerShutter));
            if (buttons == null) throw new ArgumentNullException(nameof(buttons));

            buttons.Up.GetPressedShortlyTrigger().Attach(() => HandleBlindButtonPressedEvent(rollerShutter, RollerShutterButtonDirection.Up));
            buttons.Down.GetPressedShortlyTrigger().Attach(() => HandleBlindButtonPressedEvent(rollerShutter, RollerShutterButtonDirection.Down));

            return rollerShutter;
        }

        private static void HandleBlindButtonPressedEvent(IRollerShutter rollerShutter, RollerShutterButtonDirection direction)
        {
            if (direction == RollerShutterButtonDirection.Up && rollerShutter.GetState() == RollerShutterState.MovingUp)
            {
                rollerShutter.SetState(RollerShutterState.Stopped);
            }
            else if (direction == RollerShutterButtonDirection.Down && rollerShutter.GetState() == RollerShutterState.MovingDown)
            {
                rollerShutter.SetState(RollerShutterState.Stopped);
            }
            else if (direction == RollerShutterButtonDirection.Down)
            {
                rollerShutter.SetState(RollerShutterState.MovingDown);
            }
            else if (direction == RollerShutterButtonDirection.Up)
            {
                rollerShutter.SetState(RollerShutterState.MovingUp);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
