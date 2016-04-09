using System;
using HA4IoT.Actuators.Actions;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.StateMachines
{
    public static class StateMachineExtensions
    {
        public static IArea WithStateMachine(this IArea room, Enum id, Action<StateMachine, IArea> initializer)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var stateMachine = new StateMachine(
                ComponentIdFactory.Create(room, id));

            initializer(stateMachine, room);
            stateMachine.SetInitialState(BinaryStateId.Off);

            room.AddComponent(stateMachine);
            return room;
        }

        public static IStateMachine GetStateMachine(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetComponent<IStateMachine>(ComponentIdFactory.Create(room, id));
        }

        public static bool GetSupportsOffState(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.GetSupportsState(BinaryStateId.Off);
        }

        public static bool GetSupportsOnState(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.GetSupportsState(BinaryStateId.On);
        }

        public static StateMachineState AddOffState(this StateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.AddState(BinaryStateId.Off);
        }

        public static StateMachineState AddOnState(this StateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.AddState(BinaryStateId.On);
        }

        public static StateMachineState AddState(this StateMachine stateMachine, StatefulComponentState id)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));
            if (id == null) throw new ArgumentNullException(nameof(id));

            var state = new StateMachineState(id);
            stateMachine.AddState(state);
            return state;
        }

        public static void SetNextState(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            var activeStateId = stateMachine.GetState();
            var nextStateId = stateMachine.GetNextState(activeStateId);

            stateMachine.SetState(nextStateId);
        }

        public static bool TryTurnOff(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            if (!stateMachine.GetSupportsState(BinaryStateId.Off))
            {
                return false;
            }

            stateMachine.SetState(BinaryStateId.Off);
            return true;
        }

        public static bool TryTurnOn(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            if (!stateMachine.GetSupportsState(BinaryStateId.On))
            {
                return false;
            }

            stateMachine.SetState(BinaryStateId.On);
            return true;
        }

        public static IHomeAutomationAction GetSetStateAction(this IStateMachine stateStateMachine, StatefulComponentState stateId)
        {
            if (stateStateMachine == null) throw new ArgumentNullException(nameof(stateStateMachine));
            if (stateId == null) throw new ArgumentNullException(nameof(stateId));

            return new HomeAutomationAction(() => stateStateMachine.SetState(stateId));
        }

        public static IHomeAutomationAction GetTurnOnAction(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return new HomeAutomationAction(() => stateMachine.SetState(BinaryStateId.On));
        }

        public static IHomeAutomationAction GetTurnOffAction(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return new HomeAutomationAction(() => stateMachine.SetState(BinaryStateId.Off));
        }

        public static IHomeAutomationAction GetSetNextStateAction(this IStateMachine stateStateMachine)
        {
            if (stateStateMachine == null) throw new ArgumentNullException(nameof(stateStateMachine));

            return new HomeAutomationAction(stateStateMachine.SetNextState);
        }
    }
}
