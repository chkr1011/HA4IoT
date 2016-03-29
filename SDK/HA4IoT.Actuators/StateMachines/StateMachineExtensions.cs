using System;
using HA4IoT.Actuators.Actions;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Actuators
{
    public static class StateMachineExtensions
    {
        public static IArea WithStateMachine(this IArea room, Enum id, Action<StateMachine, IArea> initializer)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (initializer == null) throw new ArgumentNullException(nameof(initializer));

            var stateMachine = new StateMachine(
                ActuatorIdFactory.Create(room, id));

            initializer(stateMachine, room);
            stateMachine.SetInitialState(DefaultStateIDs.Off);

            room.AddActuator(stateMachine);
            return room;
        }

        public static IStateMachine GetStateMachine(this IArea room, Enum id)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            return room.GetActuator<IStateMachine>(ActuatorIdFactory.Create(room, id));
        }

        public static bool GetSupportsOffState(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.GetSupportsState(DefaultStateIDs.Off);
        }

        public static bool GetSupportsOnState(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.GetSupportsState(DefaultStateIDs.On);
        }

        public static IHomeAutomationAction GetSetNextStateAction(this IStateMachine stateStateMachine)
        {
            if (stateStateMachine == null) throw new ArgumentNullException(nameof(stateStateMachine));

            return new HomeAutomationAction(stateStateMachine.SetNextState);
        }

        public static IHomeAutomationAction GetSetStateAction(this IStateMachine stateStateMachine, StateMachineStateId stateId)
        {
            if (stateStateMachine == null) throw new ArgumentNullException(nameof(stateStateMachine));
            if (stateId == null) throw new ArgumentNullException(nameof(stateId));

            return new HomeAutomationAction(() => stateStateMachine.SetActiveState(stateId));
        }

        public static StateMachineState AddOffState(this StateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.AddState(DefaultStateIDs.Off);
        }

        public static StateMachineState AddOnState(this StateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.AddState(DefaultStateIDs.On);
        }

        public static StateMachineState AddState(this StateMachine stateMachine, StateMachineStateId id)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));
            if (id == null) throw new ArgumentNullException(nameof(id));

            var state = new StateMachineState(id, stateMachine);
            stateMachine.AddState(state);
            return state;
        }

        public static void SetNextState(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            var activeStateId = stateMachine.GetActiveState();
            var nextStateId = stateMachine.GetNextState(activeStateId);

            stateMachine.SetActiveState(nextStateId);
        }

        public static bool TryTurnOff(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            if (!stateMachine.GetSupportsState(DefaultStateIDs.Off))
            {
                return false;
            }

            stateMachine.SetActiveState(DefaultStateIDs.Off);
            return true;
        }

        public static bool TryTurnOn(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            if (!stateMachine.GetSupportsState(DefaultStateIDs.On))
            {
                return false;
            }

            stateMachine.SetActiveState(DefaultStateIDs.On);
            return true;
        }

        public static IHomeAutomationAction GetTurnOnAction(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return new HomeAutomationAction(() => stateMachine.SetActiveState(DefaultStateIDs.On));
        }

        public static IHomeAutomationAction GetTurnOffAction(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return new HomeAutomationAction(() => stateMachine.SetActiveState(DefaultStateIDs.Off));
        }

        public static IHomeAutomationAction GetApplyNextStateAction(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return new HomeAutomationAction(stateMachine.SetNextState);
        }
    }
}
