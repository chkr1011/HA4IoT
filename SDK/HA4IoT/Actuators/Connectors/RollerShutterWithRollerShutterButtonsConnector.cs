using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components.States;
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

            upButton.PressedShortlyTrigger.Attach(() => HandleBlindButtonPressedEvent(rollerShutter, VerticalMovingStateValue.MovingUp));
            downButton.PressedShortlyTrigger.Attach(() => HandleBlindButtonPressedEvent(rollerShutter, VerticalMovingStateValue.MovingDown));

            return rollerShutter;
        }

        private static void HandleBlindButtonPressedEvent(IRollerShutter rollerShutter, VerticalMovingStateValue verticalMovingState)
        {
            if (verticalMovingState == VerticalMovingStateValue.MovingUp && rollerShutter.GetState().Has(VerticalMovingState.MovingUp))
            {
                rollerShutter.InvokeCommand(new TurnOffCommand());
            }
            else if (verticalMovingState == VerticalMovingStateValue.MovingDown && rollerShutter.GetState().Has(VerticalMovingState.MovingDown))
            {
                rollerShutter.InvokeCommand(new TurnOffCommand());
            }
            else if (verticalMovingState == VerticalMovingStateValue.MovingDown)
            {
                rollerShutter.InvokeCommand(new MoveDownCommand());
            }
            else if (verticalMovingState == VerticalMovingStateValue.MovingUp)
            {
                rollerShutter.InvokeCommand(new MoveUpCommand());
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
