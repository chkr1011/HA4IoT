using System;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Actuators.Connectors
{
    public static class StateMachineWithButtonConnector
    {
        public static IStateMachine ConnectMoveNextAndToggleOffWith(this IStateMachine stateMachine, IButton button)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));
            if (button == null) throw new ArgumentNullException(nameof(button));
            
            // TODO: Fix!
            //button.PressedShortlyTrigger.Attach(stateMachine.SetState.SetNextState);

            if (stateMachine.GetFeatures().Supports<PowerStateFeature>())
            {
                button.PressedLongTrigger.Attach(() => stateMachine.TryTurnOff());
            }

            return stateMachine;
        }
    }
}
