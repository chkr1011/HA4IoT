using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Actuators.Connectors
{
    public static class RollerShutterWithRollerShutterButtonsConnector
    {
        public static IRollerShutter ConnectWith(
            this IRollerShutter rollerShutter, 
            IButton upButton,
            IButton downButton)
        {
            if (rollerShutter == null) throw new ArgumentNullException(nameof(rollerShutter));
            if (upButton == null) throw new ArgumentNullException(nameof(upButton));
            if (downButton == null) throw new ArgumentNullException(nameof(downButton));

            upButton.GetPressedShortlyTrigger().Attach(() => HandleBlindButtonPressedEvent(rollerShutter, RollerShutterStateId.MovingUp));
            downButton.GetPressedShortlyTrigger().Attach(() => HandleBlindButtonPressedEvent(rollerShutter, RollerShutterStateId.MovingDown));

            return rollerShutter;
        }

        private static void HandleBlindButtonPressedEvent(IRollerShutter rollerShutter, ComponentState direction)
        {
            if (direction.Equals(RollerShutterStateId.MovingUp) && rollerShutter.GetState().Equals(RollerShutterStateId.MovingUp))
            {
                rollerShutter.SetState(RollerShutterStateId.Off);
            }
            else if (direction.Equals(RollerShutterStateId.MovingDown) && rollerShutter.GetState().Equals(RollerShutterStateId.MovingDown))
            {
                rollerShutter.SetState(RollerShutterStateId.Off);
            }
            else if (direction.Equals(RollerShutterStateId.MovingDown))
            {
                rollerShutter.SetState(RollerShutterStateId.MovingDown);
            }
            else if (direction.Equals(RollerShutterStateId.MovingUp))
            {
                rollerShutter.SetState(RollerShutterStateId.MovingUp);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
