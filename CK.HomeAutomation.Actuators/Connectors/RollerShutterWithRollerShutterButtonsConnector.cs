using System;

namespace CK.HomeAutomation.Actuators.Connectors
{
    public static class RollerShutterWithRollerShutterButtonsConnector
    {
        public static RollerShutter ConnectWith(this RollerShutter rollerShutter, RollerShutterButtons buttons)
        {
            if (rollerShutter == null) throw new ArgumentNullException(nameof(rollerShutter));
            if (buttons == null) throw new ArgumentNullException(nameof(buttons));

            buttons.Up.PressedShort += (s, e) => HandleBlindButtonPressedEvent(rollerShutter, RollerShutterButtonDirection.Up);
            buttons.Down.PressedShort += (s, e) => HandleBlindButtonPressedEvent(rollerShutter, RollerShutterButtonDirection.Down);

            return rollerShutter;
        }

        private static void HandleBlindButtonPressedEvent(RollerShutter rollerShutter, RollerShutterButtonDirection direction)
        {
            if (direction == RollerShutterButtonDirection.Up && rollerShutter.State == RollerShutterState.MovingUp)
            {
                rollerShutter.Stop();
            }
            else if (direction == RollerShutterButtonDirection.Down && rollerShutter.State == RollerShutterState.MovingDown)
            {
                rollerShutter.Stop();
            }
            else if (direction == RollerShutterButtonDirection.Down)
            {
                rollerShutter.StartMoveDown();
            }
            else if (direction == RollerShutterButtonDirection.Up)
            {
                rollerShutter.StartMoveUp();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
